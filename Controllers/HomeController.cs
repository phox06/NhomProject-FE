using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; 

namespace NhomProject.Controllers
{
    public class HomeController : Controller
    {
        // 1. Home Page
        private ApplicationDbContext _db = new ApplicationDbContext();
        public ActionResult Index()
        {
         
            var products = _db.Products.ToList();
            return View(products);
        }

        // 2. Category of Products Page
        public ActionResult Category(string id, string sortOrder)
        {
            // If no id is provided, redirect to home.
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            // 1. Find the category and include its products
            // (This is more efficient than two separate queries)
            var category = _db.Categories
                              .Include(c => c.Products) // Make sure to add 'using System.Data.Entity;'
                              .FirstOrDefault(c => c.Name.ToLower() == id.ToLower());

            // Handle if category doesn't exist
            if (category == null)
            {
                return HttpNotFound();
            }

            // 2. Pass category info to the view
            ViewData["CategoryName"] = category.Name;
            ViewData["CategoryDescription"] = category.Description; // <-- NEW
            ViewData["CurrentSort"] = sortOrder; // <-- NEW

            // 3. Get the list of products
            var products = category.Products.ToList();

            // 4. Apply sorting based on the sortOrder parameter
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
                    products = products.OrderBy(p => p.Name).ToList(); // Default sort
                    break;
            }

            // 5. Pass the filtered and sorted list of products to the view
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

        // 4. Shopping Cart Page
        public ActionResult Cart()
        {
            Cart cart = GetCart();
            return View(cart);
        }

        // 5. Sign In / Sign Up Page
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }

        // 6. Customer Shopping History Page
        public ActionResult OrderHistory()
        {
            return View();
        }

        // 7. Order Details Page
        public ActionResult OrderDetails(int id)
        {
           
            ViewData["OrderId"] = id;
            return View();
        }

        // 8. Payment/Finalize Order Page
        // 8. Payment/Finalize Order Page (GET)
        [HttpPost]
        public ActionResult Checkout(Order model) // Receives customer info from the form
        {
            Cart cart = GetCart();

            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            // 1. Create the new Order object
            var order = new Order
            {
                CustomerName = model.CustomerName,
                Address = model.Address,
                Phone = model.Phone,
                Date = DateTime.Now,
                Status = "Pending",
                Total = cart.GetTotal() // Use Total from order.cs
            };

            // 2. Add the cart items to the order
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

            // 3. Save the order to the database
            _db.Orders.Add(order);
            _db.SaveChanges();

            // 4. Clear the user's cart
            Session["Cart"] = null;

            // 5. Redirect to a confirmation page
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }
        // 10. Order Confirmation Page
        public ActionResult OrderConfirmation(int id)
        {
            // Pass the Order ID to the view
            ViewBag.OrderId = id;
            return View();
        }
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
        // 8. Payment/Finalize Order Page (GET)
        public ActionResult Checkout()
        {
            Cart cart = GetCart();

            // If cart is empty, redirect them back to the cart page
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            ViewBag.Cart = cart; // Pass the cart to the view for summary
            return View();
        }
        [HttpPost] // This action is called by a form submission
        public ActionResult AddToCart(int id, int quantity)
        {
            // Find the product in the database
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                // Get the cart from the session
                Cart cart = GetCart();
                // Add the product to the cart
                cart.AddItem(product, quantity);
            }

            // Redirect the user to their cart page
            return RedirectToAction("Cart");
        }

        // ACTION TO REMOVE AN ITEM FROM THE CART
        public ActionResult RemoveFromCart(int id)
        {
            Cart cart = GetCart();
            cart.RemoveItem(id);

            // Redirect back to the cart page
            return RedirectToAction("Cart");
        }
    }
}