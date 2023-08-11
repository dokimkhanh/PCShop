using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Product
        public ActionResult Index(int? id)
        {
            var items = dbContext.products.OrderByDescending(x => x.id).ToList();
            if (id != null)
            {
                items = items.Where(x => x.id == id).ToList();
            }
            return View(items);
        }

        public ActionResult ProductCategory(string alias, int? id)
        {
            var items = dbContext.products.OrderByDescending(x => x.id).ToList();
            if (id != null)
            {
                items = items.Where(x => x.categoryID == id).ToList();
            }
            ViewBag.CategoryID = id;
            return View(items);
        }

        public ActionResult ItemByCategory()
        {
            var items = dbContext.products.OrderByDescending(x => x.id).ToList();
            return PartialView("_ItemByCategory", items);
        }

        public ActionResult ItemProductSale()
        {
            var item = dbContext.products.OrderByDescending(x => x.id).Where(x => x.isSale).ToList();
            return PartialView("_ItemProductSale", item);
        }

        public ActionResult ProductDetail(string alias, int id)
        {
            var item = dbContext.products.Find(id);
            return View(item);
        }
    }
}