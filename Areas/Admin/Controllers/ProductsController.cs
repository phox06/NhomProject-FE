using NhomProject.Models;
using NhomProject.Models.ViewModel;
using PagedList;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NhomProject.Areas.Admin.Controllers
{
    public class ProductsController : BaseController
    {
        private MyProjectDatabaseEntities db = new MyProjectDatabaseEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.Include(p => p.Category)
                              .Where(p => p.IsActive == true)
                              .AsQueryable();


            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.Category.Name.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

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


            int pageNumber = page ?? 1;
            int pageSize = 10;


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
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");


            var newProduct = new Product();
            newProduct.IsActive = true;

            return View(newProduct);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase UploadImage)
        {
            if (ModelState.IsValid)
            {

                if (UploadImage != null && UploadImage.ContentLength > 0)
                {

                    string filename = Path.GetFileName(UploadImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), filename);
                    UploadImage.SaveAs(path);


                    product.MainImageUrl = filename;
                }


                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

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
                if (UploadImg != null && UploadImg.ContentLength > 0)
                {
                    string filename = Path.GetFileName(UploadImg.FileName);
                    string savePath = "~/Content/images/";
                    product.MainImageUrl = savePath + filename;
                    UploadImg.SaveAs(Path.Combine(Server.MapPath(savePath), filename));
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        public ActionResult ManageImages(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadImages(int id, HttpPostedFileBase[] files)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {

                        string fileName = System.IO.Path.GetFileName(file.FileName);
                        string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                        file.SaveAs(path);


                        var img = new NhomProject.Models.ProductImage();
                        img.ProductId = id;
                        img.ImageUrl = "~/Content/images/" + fileName;

                        db.ProductImages.Add(img);
                    }
                }
                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Đã tải lên hình ảnh thành công!";
            return RedirectToAction("ManageImages", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddImageUrl(int id, string imageUrl)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            if (!string.IsNullOrEmpty(imageUrl))
            {
                var img = new NhomProject.Models.ProductImage();
                img.ProductId = id;
                img.ImageUrl = imageUrl;

                db.ProductImages.Add(img);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã thêm link ảnh thành công!";
            }

            return RedirectToAction("ManageImages", new { id = id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImage(int id)
        {
            var img = db.ProductImages.Find(id);
            if (img != null)
            {
                int productId = img.ProductId;
                db.ProductImages.Remove(img);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa ảnh thành công.";
                return RedirectToAction("ManageImages", new { id = productId });
            }
            return RedirectToAction("Index");
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

            var orderDetails = db.OrderDetails.Where(od => od.ProductId == id).ToList();

            if (orderDetails.Count == 0)
            {

                var images = db.ProductImages.Where(i => i.ProductId == id).ToList();
                foreach (var img in images)
                {
                    db.ProductImages.Remove(img);
                }

                db.Products.Remove(product);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì nó đang tồn tại trong các đơn hàng cũ.";
                return RedirectToAction("Index");
            }
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