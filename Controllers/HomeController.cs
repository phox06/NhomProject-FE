using NhomProject.Models;
using NhomProject.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace NhomProject.Controllers
{
    public class HomeController : Controller
    {
        private MyProjectDatabaseEntities _db = new MyProjectDatabaseEntities();

        // --- 1. USE THE NEW CART SERVICE ---
        private CartService _cartService = new CartService();

        // (We removed the old "GetCart" method because the Service handles it now)

        public ActionResult Index()
        {
            // Only show ACTIVE products
            var allProducts = _db.Products.Include(p => p.Category)
                                 .Where(p => p.IsActive)
                                 .ToList();

            var viewModel = new HomeViewModel
            {
                FlashSaleProducts = allProducts,
                PhoneProducts = allProducts
                                    .Where(p => p.Category != null &&
                                                p.Category.Name.ToLower() == "điện thoại")
                                    .ToList(),
                LaptopProducts = allProducts
                                    .Where(p => p.Category != null &&
                                                p.Category.Name.ToLower() == "laptops")
                                    .ToList()
            };

            return View(viewModel);
        }

        // --- CART ACTIONS (UPDATED TO USE SERVICE) ---

        public ActionResult Cart()
        {
            // Load Cart from Service (DB or Session)
            Cart cart = _cartService.GetCart();
            ViewBag.Error = TempData["Error"];
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity, string redirectType)
        {
            var product = _db.Products.Find(productId);
            if (product != null)
            {
                // Use Service to Save (Handles both DB and Session)
                _cartService.AddToCart(product, quantity);

                TempData["SuccessMessage"] = "Đã thêm " + product.Name + " vào giỏ hàng!";
            }

            if (redirectType == "buynow")
            {
                return RedirectToAction("Cart");
            }
            else
            {
                return RedirectToAction("Details", new { id = productId });
            }
        }

        [HttpPost]
        public ActionResult UpdateCart(int productId, int quantity)
        {
            // Logic to update DB or Session based on login status
            Cart cart = _cartService.GetCart();
            cart.UpdateQuantity(productId, quantity);

            if (Session["UserId"] != null)
            {
                int userId = (int)Session["UserId"];
                var dbItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
                if (dbItem != null)
                {
                    dbItem.Quantity = quantity;
                    _db.SaveChanges();
                }
            }
            else
            {
                Session["Cart"] = cart;
            }

            return RedirectToAction("Cart");
        }

        public ActionResult RemoveFromCart(int id)
        {
            // Use Service to Remove from DB/Session
            _cartService.RemoveFromCart(id);
            return RedirectToAction("Cart");
        }

        // --- CHECKOUT & ORDER LOGIC ---

        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Checkout", "Home") });
            }

            Cart cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var userId = (int)Session["UserId"];
            var user = _db.Users.Find(userId);

            var model = new Order
            {
                CustomerName = user.FullName,
                Address = user.Address,
                Phone = user.Phone
            };

            ViewBag.Cart = cart;
            return View(model);
        }

        [HttpPost]
        public ActionResult Checkout(Order model)
        {
            // 1. Check Login
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 2. Get Cart
            Cart cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            // ====================================================
            // CRITICAL FIX: CALCULATE TOTAL IMMEDIATELY
            // ====================================================
            // The 'model' comes from the form with Name/Address only.
            // The 'Total' is 0 by default. We MUST set it here.
            model.Total = cart.GetTotal();

            // Set other system fields
            model.Date = DateTime.Now;
            model.Status = "Pending";
            model.UserId = userId;

            // 3. Fill the Products Lists (For both History and Admin)
            // We initialize them to ensure they aren't null
            if (model.CartItems == null) model.CartItems = new List<CartItem>();
            if (model.OrderDetails == null) model.OrderDetails = new List<OrderDetail>();

            foreach (var item in cart.Items)
            {
                // Add to User History list
                model.CartItems.Add(new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ImageUrl = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                });

                // Add to Admin OrderDetails list
                model.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            // 4. NOW Check Payment Method
            if (model.PaymentMethod == "PayPal")
            {
                // Now when we save to Session, 'model.Total' is correct (not 0)
                Session["OrderModel"] = model;
                return RedirectToAction("CreatePayment", "Paypal");
            }
            else
            {
                // COD / Cash Payment
                _db.Orders.Add(model);
                _db.SaveChanges();

                // Clear Cart
                _cartService.ClearCart();

                return RedirectToAction("OrderConfirmation", new { id = model.Id });
            }
        }

        // --- OTHER PAGES (UNCHANGED BUT CLEANED UP) ---

        public ActionResult Category(string id, string sortOrder)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var category = _db.Categories.Include(c => c.Products)
                              .FirstOrDefault(c => c.Name.ToLower() == id.ToLower());
            if (category == null) return HttpNotFound();

            ViewData["CategoryName"] = category.Name;
            ViewData["CategoryDescription"] = category.Description;
            ViewData["CurrentSort"] = sortOrder;

            // Only show Active products
            var products = category.Products.Where(p => p.IsActive).ToList();

            switch (sortOrder)
            {
                case "price_asc": products = products.OrderBy(p => p.Price).ToList(); break;
                case "price_desc": products = products.OrderByDescending(p => p.Price).ToList(); break;
                case "name_asc": products = products.OrderBy(p => p.Name).ToList(); break;
                default: products = products.OrderBy(p => p.Name).ToList(); break;
            }
            return View(products);
        }

        public ActionResult Details(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var model = new ProductDetailVM
            {
                Product = product,
                Quantity = 1,
                // Only suggest active products
                RelatedProducts = _db.Products
                                     .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.IsActive)
                                     .Take(4).ToList(),
                TopProducts = _db.Products
                                 .Where(p => p.IsActive)
                                 .OrderByDescending(p => p.Price)
                                 .Take(4).ToList()
            };

            return View(model);
        }

        public ActionResult OrderHistory()
        {
            var userId = Session["UserId"] as int?;
            if (userId == null) return RedirectToAction("Login", "Auth");

            var orders = _db.Orders.Where(o => o.UserId == userId)
                            .OrderByDescending(o => o.Date).ToList();
            return View(orders);
        }

        public ActionResult OrderDetails(int id)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null) return RedirectToAction("Login", "Auth");

            var order = _db.Orders.Include(o => o.CartItems)
                           .FirstOrDefault(o => o.Id == id && o.UserId == userId);
            if (order == null) return HttpNotFound();

            return View(order);
        }

        public ActionResult Search(string term)
        {
            var products = new List<Product>();
            ViewData["SearchTerm"] = term;
            if (!string.IsNullOrEmpty(term))
            {
                string searchTerm = term.ToLower();
                // Filter by Active status
                products = _db.Products
                               .Where(p => p.IsActive &&
                                          (p.Name.ToLower().Contains(searchTerm) ||
                                           p.Description.ToLower().Contains(searchTerm)))
                               .ToList();
            }
            return View(products);
        }

        public ActionResult OrderConfirmation(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}