using NhomProject.Models;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace NhomProject.Areas.Admin.Controllers
{
    public class CustomerController : BaseController
    {
        private MyProjectDatabaseEntities db = new MyProjectDatabaseEntities();

        // GET: Admin/Customer
        public ActionResult Index(string searchTerm, int? page)
        {
            int pageSize = 10; // 10 khách hàng mỗi trang
            int pageNumber = (page ?? 1);

            // Bắt đầu truy vấn, bao gồm cả thông tin User (để lấy Username)
            var customers = db.Users.AsQueryable();

            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                customers = customers.Where(c => c.Username.Contains(searchTerm) ||
                                                  c.Email.Contains(searchTerm) ||
                                                  c.Username.Contains(searchTerm));
            }

            // Sắp xếp (ví dụ: theo tên) và phân trang
            var pagedCustomers = customers.OrderBy(c => c.Username)
                                            .ToPagedList(pageNumber, pageSize);

            // Gửi searchTerm lại cho View để giữ giá trị trong ô tìm kiếm
            ViewBag.SearchTerm = searchTerm;

            return View(pagedCustomers);
        }

        // GET: Admin/Customer/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tải thông tin Customer, VÀ tải cả danh sách Orders liên quan
            User customer = db.Users
                                   .Include(c => c.Orders)
                                   .SingleOrDefault(c => c.UserId == id);

            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Admin/Customer/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,Email,Address,Username,Password,FullName,Phone,CreatedDate,IsActive,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Admin/Customer/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Customer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Email,Address,Username,Password,FullName,Phone,CreatedDate,IsActive,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Customer/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
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
