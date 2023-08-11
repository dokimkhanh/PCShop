using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]

    public class ProductImageController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        // GET: Admin/ProductImage
        public ActionResult Index(int id_product)
        {
            ViewBag.ProductID = id_product;
            var item = dbContext.productImages.Where(x => x.ProductId == id_product).ToList();
            return View(item);
        }

        public ActionResult AddImage(int productID, string url)
        {
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

        //[HttpPost]
        //public ActionResult Delete(int idProduct)
        //{
        //    var item = dbContext.productImages.Find(idProduct);
        //    if (item != null)
        //    {
        //        dbContext.productImages.RemoveRange(dbContext.productImages.Where(x => x.ProductId == idProduct));
        //        dbContext.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });

        //}
    }
}