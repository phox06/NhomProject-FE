using Newtonsoft.Json;
using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NhomProject.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Orders
        public ActionResult Index()
        {
            var orders = Session["Orders"] as List<Order> ?? new List<Order>();
            return View(orders);
        }

        public ActionResult Detail(int id)
        {
            if (TempData["Order"] != null)
            {
                var order = JsonConvert.DeserializeObject<Order>(TempData["Order"].ToString());
                return View(order);
            }

            // Nếu không có dữ liệu tạm (truy cập trực tiếp)
            return RedirectToAction("Index", "Home");
        }

    }
}