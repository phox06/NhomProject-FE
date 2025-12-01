using NhomProject.Models;
using PagedList;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NhomProject.Areas.Admin.Controllers
{
    public class OrdersController : BaseController
    {
        private MyProjectDatabaseEntities db = new MyProjectDatabaseEntities();

        // GET: Admin/Orders
        public ActionResult Index(string searchName, string searchStatus, string searchDate, int? page)
        {
            var orders = db.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                orders = orders.Where(o => o.CustomerName.Contains(searchName) || o.User.Username.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchStatus))
            {
                orders = orders.Where(o => o.Status == searchStatus);
            }

            if (!string.IsNullOrEmpty(searchDate) && DateTime.TryParse(searchDate, out DateTime dateValue))
            {
                orders = orders.Where(o => DbFunctions.TruncateTime(o.Date) == dateValue.Date);
            }

            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var pagedOrders = orders.OrderByDescending(o => o.Date).ToPagedList(pageNumber, pageSize);

            ViewBag.SearchName = searchName;
            ViewBag.SearchStatus = searchStatus;
            ViewBag.SearchDate = searchDate;

            return View(pagedOrders);
        }
        // GET: Admin/Orders/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            Order order = db.Orders.Find(id);
            if (order != null)
            {
                order.Status = status; 
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng #" + id + " thành công!";
                return RedirectToAction("Details", new { id = id });
            }
            return View(order);
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            Order order = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
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
