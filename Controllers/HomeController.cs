using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity; // <-- Make sure to add this
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NhomProject.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // Helper method to get the current cart from the Session
        private Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // 1. Home Page
        public ActionResult Index()
        {
            var allProducts = _db.Products.Include(p => p.Category).ToList();

            var viewModel = new HomeViewModel
            {
                // Flash Sale vẫn là tất cả sản phẩm
                FlashSaleProducts = allProducts,

                // --- SỬA LỖI ---
                // Thay đổi "phones" thành "điện thoại" (hoặc bất cứ tên nào bạn dùng trong CSDL)
                PhoneProducts = allProducts
                                    .Where(p => p.Category != null &&
                                                p.Category.Name.ToLower() == "điện thoại")
                                    .ToList(),

                // --- SỬA LỖI ---
                // Thay đổi "laptops" thành "laptop"
                LaptopProducts = allProducts
                                    .Where(p => p.Category != null &&
                                                p.Category.Name.ToLower() == "laptops")
                                    .ToList()
            };

            return View(viewModel);
        }


        // 2. Category of Products Page
        public ActionResult Category(string id, string sortOrder)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            var category = _db.Categories
                              .Include(c => c.Products)
                              .FirstOrDefault(c => c.Name.ToLower() == id.ToLower());
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewData["CategoryName"] = category.Name;
            ViewData["CategoryDescription"] = category.Description;
            ViewData["CurrentSort"] = sortOrder;
            var products = category.Products.ToList();
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
                default:
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
            }
            return View(products);
        }

        // 3. Product Details Page
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            // Use ProductId, which matches your views
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // 4. Shopping Cart Page
        public ActionResult Cart()
        {
            Cart cart = GetCart();
            return View(cart);
        }

        // ACTION TO ADD AN ITEM TO THE CART
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            // Use ProductId
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                Cart cart = GetCart();
                cart.AddItem(product, quantity);
            }
            return RedirectToAction("Cart");
        }

        // ACTION TO REMOVE AN ITEM FROM THE CART
        public ActionResult RemoveFromCart(int id)
        {
            Cart cart = GetCart();
            cart.RemoveItem(id); // Assumes Cart.RemoveItem uses ProductId
            return RedirectToAction("Cart");
        }

        // 5. SIGN IN / SIGN UP ACTIONS (REMOVED)
        // These are now handled by AuthController.cs

        // 6. Customer Shopping History Page
        public ActionResult OrderHistory()
        {
            // Check if user is logged in
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                // If not, redirect to login
                return RedirectToAction("Login", "Auth");
            }

            // Find orders for the logged-in user and show newest first
            var orders = _db.Orders
                            .Where(o => o.UserId == userId)
                            .OrderByDescending(o => o.Date)
                            .ToList();

            return View(orders);
        }

        // 7. Order Details Page
        public ActionResult OrderDetails(int id)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Find the order AND check if it belongs to the logged-in user
            var order = _db.Orders
                           .Include(o => o.Items) // Load the products
                           .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                // If no order, or order doesn't belong to user, return Not Found
                return HttpNotFound();
            }

            return View(order);
        }

        // 8. Payment/Finalize Order Page (GET)
        public ActionResult Checkout()
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                // If not, redirect to login and pass the current page as returnUrl
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Checkout", "Home") });
            }

            Cart cart = GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            // --- NEW: Auto-fill checkout form ---
            var userId = (int)Session["UserId"];
            var user = _db.Users.Find(userId);

            var model = new Order
            {
                CustomerName = user.FullName, // Use FullName
                Address = user.Address,
                Phone = user.Phone
            };

            ViewBag.Cart = cart;
            return View(model); // Pass the pre-filled model to the view
        }

        // 8. Payment/Finalize Order Page (POST)
        [HttpPost]
        public ActionResult Checkout(Order model)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            Cart cart = GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var order = new Order
            {
                CustomerName = model.CustomerName,
                Address = model.Address,
                Phone = model.Phone,
                PaymentMethod = model.PaymentMethod,
                Date = DateTime.Now,
                Status = "Pending",
                Total = cart.GetTotal(),
                UserId = userId // <-- LINK THE ORDER TO THE USER
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ImageUrl = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }

            _db.Orders.Add(order);
            _db.SaveChanges();

            Session["Cart"] = null;

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // 9. Search Results Page
        public ActionResult Search(string term)
        {
            var products = new List<Products>();
            ViewData["SearchTerm"] = term;
            if (!string.IsNullOrEmpty(term))
            {
                string searchTerm = term.ToLower();
                products = _db.Products
                               .Where(p => p.Name.ToLower().Contains(searchTerm) ||
                                           p.Description.ToLower().Contains(searchTerm))
                               .ToList();
            }
            return View(products);
        }

        // 10. Order Confirmation Page
        public ActionResult OrderConfirmation(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // Make sure to dispose the db context
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

