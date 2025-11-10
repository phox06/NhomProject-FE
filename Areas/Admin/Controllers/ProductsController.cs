using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NhomProject.Models;
using NhomProject.Areas.Admin.Model.ViewModel;
using PagedList;
using System.IO;

namespace NhomProject.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index()
        {
            return View();
        }
    }
}