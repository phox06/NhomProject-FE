using NhomProject.Models;
using NhomProject.Models.ViewModel;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace NhomProject.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        private MyProjectDatabaseEntities db = new MyProjectDatabaseEntities();


        public ActionResult Index()
        {

            var stats = db.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryStatisticVM
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count(),


                    HighestPrice = c.Products.Any() ? c.Products.Max(p => p.Price) : 0,
                    LowestPrice = c.Products.Any() ? c.Products.Min(p => p.Price) : 0,
                    AveragePrice = c.Products.Any() ? c.Products.Average(p => p.Price) : 0
                }).ToList();


            return View(stats);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new NhomProject.Models.User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(NhomProject.Models.User model)
        {
            if (ModelState.IsValid)
            {

                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                      && u.Password == model.Password
                                                      && u.UserRole == "Admin");

                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập, mật khẩu không đúng hoặc bạn không có quyền Admin.");
                }
            }

            return View(model);
        }

        // GET: Admin/Home/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}