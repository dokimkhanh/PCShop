using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class MenuController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuArrivals()
        {
            var item = dbContext.categories.ToList();
            return PartialView("_MenuArrivals", item);
        }

        public ActionResult MenuProductCategoryLeft(int? id)
        {
            if (id != null)
            {
                ViewBag.CategoryID = id;
            }
            var item = dbContext.categories.ToList();
            return PartialView("_MenuProductCategoryLeft", item);
        }
    }
}