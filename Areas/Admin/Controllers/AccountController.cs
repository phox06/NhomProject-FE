using NhomProject.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
namespace NhomProject.Areas.Admin.Controllers
{
    public class AccountController : BaseController
    {
        private MyProjectDatabaseEntities db = new MyProjectDatabaseEntities();
        // GET: Admin/Account
        public ActionResult Index()
        {

            var adminUsers = db.Users.Where(u => u.UserRole == "Admin").ToList();
            return View(adminUsers);
        }

        // GET: Admin/AdminUsers/Details/admin_username
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.SingleOrDefault(u => u.Username == id && u.UserRole == "Admin");
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/AdminUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (db.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View(user);
                }

                // --- START OF FIX ---
                // Manually add all the other required fields

                // 1. Set the role
                user.UserRole = "Admin";

                // 2. Set the created date
                user.CreatedDate = System.DateTime.Now;

                // 3. Set the account to active
                user.IsActive = true;

                // 4. Set default values for other required fields
                // (We can just use the username as a default for these)
                user.FullName = user.Username;
                user.Email = user.Username + "@admin.com"; // Or any default
                                                           // --- END OF FIX ---

                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Admin/AdminUsers/Edit/admin_username
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.SingleOrDefault(u => u.Username == id && u.UserRole == "Admin");
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/AdminUsers/Edit/admin_username
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password")] User user)
        {
            if (ModelState.IsValid)
            {

                user.UserRole = "Admin";
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/AdminUsers/Delete/admin_username
        public ActionResult Delete(int? id) // <-- CHANGED from string to int?
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // CHANGED from u.Username == id to u.UserId == id.Value
            User user = db.Users.SingleOrDefault(u => u.UserId == id.Value && u.UserRole == "Admin");
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/AdminUsers/Delete/admin_username
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) // <-- CHANGED from string to int
        {
            // CHANGED from u.Username == id to u.UserId == id
            User user = db.Users.SingleOrDefault(u => u.UserId == id && u.UserRole == "Admin");
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
