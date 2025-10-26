using Newtonsoft.Json;
using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NhomProject.Controllers
{
    public class CheckoutController : Controller
    {
        // GET: Checkout
        public ActionResult Index()
        {
            var cart = Session["Cart"] as List<CartItem>;
            decimal total = 0;

            if (cart != null)
            {
                total = cart.Sum(x => x.Price * x.Quantity);
            }

            ViewBag.Total = total;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string customerName, string address, string phone, string payment)
        {
            
            // Kiểm tra nhập thiếu
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "⚠️ Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // Kiểm tra số điện thoại không đủ 10 số
            if (phone.Length != 10)
            {
                ViewBag.Error = "⚠️ Số điện thoại phải đủ 10 số!";
                return View();
            }

            // Lấy dữ liệu giỏ hàng từ Session (hoặc tạo tạm nếu chưa có)
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>
            {
                new CartItem { ProductName = "Samsung Galaxy S23", Quantity = 1, Price = 20000000 }
            };
            decimal total = 0;
            foreach (var item in cart)
                total += item.Price * item.Quantity;

            var order = new Order
            {
                Id = new Random().Next(1000, 9999),
                CustomerName = customerName,
                Address = address,
                Phone = phone,
                PaymentMethod = payment,
                Date = DateTime.Now,
                Items = cart,
                Total = cart.Sum(i => i.Price * i.Quantity)
            };

            // Lưu sang TempData để qua trang chi tiết đơn hàng
            TempData["Order"] = JsonConvert.SerializeObject(order);

            // Xóa giỏ hàng sau khi đặt
            Session["Cart"] = null;

            return RedirectToAction("Detail", "Orders", new { id = order.Id });
        }
    }
}