using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NhomProject.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = new List<CartItem>
            {
                new CartItem { Id = 1, ProductName = "iPhone 15", Quantity = 1, Price = 30000000 },
                new CartItem { Id = 2, ProductName = "AirPods Pro", Quantity = 2, Price = 5000000 }
            };

            return View(cart);
        }
    }
}