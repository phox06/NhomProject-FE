using NhomProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace NhomProject.Controllers
{
    public class AuthController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Auth/Login
        public ActionResult Login()
        {
            
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password && u.IsActive);

            if (user != null)
            {
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["FullName"] = user.FullName;
                Session["Email"] = user.Email;

               
                string returnUrl = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }
        }

        // GET: Auth/Register
        public ActionResult Register()
        {
            
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                
                if (_db.Users.Any(u => u.Username == user.Username))
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại";
                    return View(user);
                }

                
                if (_db.Users.Any(u => u.Email == user.Email))
                {
                    ViewBag.Error = "Email đã tồn tại";
                    return View(user);
                }

                user.CreatedDate = DateTime.Now;
                user.IsActive = true;

                _db.Users.Add(user);
                _db.SaveChanges();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Auth/Profile
        public ActionResult Profile()
        {
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            
            var viewModel = new ProfileViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address
            };

            
            ViewBag.CreatedDate = user.CreatedDate;
            return View(viewModel); 
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public ActionResult Profile(ProfileViewModel model)
        {
            var userId = Session["UserId"] as int?;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var existingUser = _db.Users.Find(userId);
            if (existingUser == null)
            {
                return HttpNotFound();
            }

            
            ViewBag.CreatedDate = existingUser.CreatedDate;

            
            if (_db.Users.Any(u => u.Email == model.Email && u.UserId != userId))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                
                existingUser.FullName = model.FullName;
                existingUser.Email = model.Email;
                existingUser.Phone = model.Phone;
                existingUser.Address = model.Address;

                
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    
                    existingUser.Password = System.Web.Helpers.Crypto.HashPassword(model.NewPassword);
                }

                _db.SaveChanges();
                ViewBag.Success = "Cập nhật thông tin thành công";

                
                Session["FullName"] = existingUser.FullName;
                Session["Email"] = existingUser.Email;
            }

            return View(model); 
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
