using PCShop.Models.Common;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class MenuController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuArrivals()
        {
            var roots = CategoryMenuHelper.GetMenuTree();
            return PartialView("_MenuArrivals", roots);
        }

        public ActionResult MenuProductCategoryLeft(int? id)
        {
            if (id != null)
            {
                ViewBag.CategoryID = id;
            }
            var tree = CategoryMenuHelper.GetMenuTree();
            return PartialView("_MenuProductCategoryLeft", tree);
        }
    }
}
