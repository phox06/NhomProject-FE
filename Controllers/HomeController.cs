using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity; 
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NhomProject.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        
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

        
        public ActionResult Index()
        {
            var allProducts = _db.Products.Include(p => p.Category).ToList();

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

       
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        
        public ActionResult Cart()
        {
            Cart cart = GetCart();
            return View(cart);
        }

        
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                Cart cart = GetCart();
                cart.AddItem(product, quantity);
            }
            return RedirectToAction("Cart");
        }

        
        public ActionResult RemoveFromCart(int id)
        {
            Cart cart = GetCart();
            cart.RemoveItem(id); 
            return RedirectToAction("Cart");
        }

       
        public ActionResult OrderHistory()
        {
            
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                
                return RedirectToAction("Login", "Auth");
            }

            
            var orders = _db.Orders
                            .Where(o => o.UserId == userId)
                            .OrderByDescending(o => o.Date)
                            .ToList();

            return View(orders);
        }

       
        public ActionResult OrderDetails(int id)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = _db.Orders
                           .Include(o => o.Items)
                           .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        
        public ActionResult Checkout()
        {
            
            if (Session["UserId"] == null)
            {
               
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Checkout", "Home") });
            }

            Cart cart = GetCart();
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
                UserId = userId 
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

       
        public ActionResult OrderConfirmation(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

       
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

