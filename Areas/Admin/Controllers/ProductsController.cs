using NhomProject.Models;
using NhomProject.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace NhomProject.Areas.Admin.Controllers
{
    public class ProductsController : BaseController
    {
        private MyProjectDatabaseEntities db = new MyProjectDatabaseEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.Include(p => p.Category).AsQueryable();

            // Tìm kiếm sản phẩm dựa trên từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.Category.Name.Contains(searchTerm));
            }

            // Tìm kiếm theo giá tối thiểu
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            // Tìm kiếm theo giá tối đa
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Sắp xếp kết quả
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Đoạn code liên quan tới phân trang
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số sản phẩm mỗi trang

            // Convert to list first, then to paged list
            var productList = products.ToList();
            model.Products = new StaticPagedList<Product>(
                productList.Skip((pageNumber - 1) * pageSize).Take(pageSize),
                pageNumber,
                pageSize,
                productList.Count
            );
            model.SearchTerm = searchTerm;
            model.MinPrice = minPrice;
            model.MaxPrice = maxPrice;
            model.SortOrder = sortOrder;
            model.PageNumber = pageNumber;
            model.PageSize = pageSize;

            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest); // mã lỗi 400: thiếu giá trị truyền vào
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound(); // mã lỗi 404
            }
            return View(product);
        }
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            return View(new Product());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,Name,Description,Price,OldPrice,CategoryId,Rating,ReviewCount,ThumbnailUrl,MainImageUrl")] Product product, HttpPostedFileBase UploadImg)
        {
            if (ModelState.IsValid)
            {
                // Thêm đoạn code để lấy đường link của ảnh đã upload và lưu ảnh vào thư mục Content/images
                if (UploadImg != null && UploadImg.ContentLength > 0)
                {
                    string filename = Path.GetFileName(UploadImg.FileName);
                    string savePath = "~/Content/images/";
                    product.MainImageUrl = savePath + filename;
                    UploadImg.SaveAs(Path.Combine(Server.MapPath(savePath), filename));
                }
                else
                {
                    // Nếu không upload ảnh, sử dụng ảnh mặc định
                    product.MainImageUrl = product.MainImageUrl ?? "~/Content/images/default_img.png";
                }

                // Set default values for Rating and ReviewCount if not provided
                if (product.Rating == 0)
                    product.Rating = 0;
                if (product.ReviewCount == 0)
                    product.ReviewCount = 0;

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,Name,Description,Price,OldPrice,CategoryId,Rating,ReviewCount,ThumbnailUrl,MainImageUrl")] Product product, HttpPostedFileBase UploadImg)
        {
            if (ModelState.IsValid)
            {
                // Thêm đoạn code để lấy đường link của ảnh đã upload và lưu ảnh vào thư mục Content/images
                if (UploadImg != null && UploadImg.ContentLength > 0)
                {
                    string filename = Path.GetFileName(UploadImg.FileName);
                    string savePath = "~/Content/images/";
                    product.MainImageUrl = savePath + filename;
                    UploadImg.SaveAs(Path.Combine(Server.MapPath(savePath), filename));
                }
                // If no new image uploaded, keep existing MainImageUrl (already in model)

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
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