using PCShop.Models;
using PCShop.Models.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class ProductImageController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("Product.View", "Product.Manage")]
        public ActionResult Index(int id_product)
        {
            ViewBag.ProductID = id_product;
            var item = dbContext.productImages.Where(x => x.ProductId == id_product).ToList();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult AddImage(int productID, string url)
        {
            if (productID <= 0 || string.IsNullOrWhiteSpace(url))
            {
                return Json(new { success = false, msg = "Dữ liệu ảnh không hợp lệ." });
            }

            url = url.Trim();
            if (url.Length > 500)
            {
                return Json(new { success = false, msg = "Đường dẫn ảnh quá dài." });
            }

            if (!dbContext.products.Any(x => x.id == productID))
            {
                return Json(new { success = false, msg = "Sản phẩm không tồn tại." });
            }

            dbContext.productImages.Add(new Models.EF.ProductImage
            {
                ProductId = productID,
                image = url,
                isDefault = false
            });
            dbContext.SaveChanges();
            return Json(new { success = true });
        }


        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult Delete(int id)
        {
            var item = dbContext.productImages.Find(id);
            if (item != null)
            {
                dbContext.productImages.Remove(item);
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }


    }
}