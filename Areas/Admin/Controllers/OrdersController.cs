using NhomProject.Models;
using PagedList;
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
        public ActionResult Index(string searchTerm, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);


            var orders = db.Orders.Include(o => o.User).AsQueryable();


            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => o.User.Username.Contains(searchTerm) ||
                                           o.Status.Contains(searchTerm));
            }


            var pagedOrders = orders.OrderByDescending(o => o.Date)
                                    .ToPagedList(pageNumber, pageSize);


            ViewBag.SearchTerm = searchTerm;

            return View(pagedOrders);
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
