using NhomProject.Models;
using NhomProject.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace NhomProject.Controllers
{
    public class HomeController : Controller
    {
        private MyProjectDatabaseEntities _db = new MyProjectDatabaseEntities();

        private CartService _cartService = new CartService();
        public ActionResult Index()
        {
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
        public ActionResult Cart()
        {
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
            _cartService.RemoveFromCart(id);
            return RedirectToAction("Cart");
        }
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
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            Cart cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }
            model.Total = cart.GetTotal();
            model.Date = DateTime.Now;
            model.Status = "Pending";
            model.UserId = userId;

            if (model.CartItems == null) model.CartItems = new List<CartItem>();
            if (model.OrderDetails == null) model.OrderDetails = new List<OrderDetail>();

            foreach (var item in cart.Items)
            {
                model.CartItems.Add(new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ImageUrl = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                });

                model.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            if (model.PaymentMethod == "PayPal")
            {

                Session["OrderModel"] = model;
                return RedirectToAction("CreatePayment", "Paypal");
            }
            else
            {

                _db.Orders.Add(model);
                _db.SaveChanges();

                _cartService.ClearCart();

                return RedirectToAction("OrderConfirmation", new { id = model.Id });
            }
        }
        public ActionResult Category(string id, string sortOrder)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var category = _db.Categories.Include(c => c.Products)
                              .FirstOrDefault(c => c.Name.ToLower() == id.ToLower());
            if (category == null) return HttpNotFound();

            ViewData["CategoryName"] = category.Name;
            ViewData["CategoryDescription"] = category.Description;
            ViewData["CurrentSort"] = sortOrder;

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

        public ActionResult OrderHistory(string searchTerm)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null) return RedirectToAction("Login", "Auth");

            var orders = _db.Orders.Include(o => o.CartItems) 
                                   .Where(o => o.UserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int orderId))
                {
                    orders = orders.Where(o => o.Id == orderId);
                }
                else
                {
                    orders = orders.Where(o => o.CartItems.Any(i => i.ProductName.Contains(searchTerm)));
                }
            }

            var orderList = orders.OrderByDescending(o => o.Date).ToList();
            ViewBag.SearchTerm = searchTerm;

            return View(orderList);
        }

        public ActionResult BuyAgain(int orderId)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null) return RedirectToAction("Login", "Auth");

            var orderItems = _db.CartItems.Where(c => c.OrderId == orderId).ToList();

            foreach (var item in orderItems)
            {
                var product = _db.Products.Find(item.ProductId);
                if (product != null)
                {
                    _cartService.AddToCart(product, 1);
                }
            }

            return RedirectToAction("Cart");
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