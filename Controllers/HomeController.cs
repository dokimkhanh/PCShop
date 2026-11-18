using PCShop.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Health(string db)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            var checkDb = string.Equals((db ?? string.Empty).Trim(), "1", StringComparison.Ordinal)
                || string.Equals((db ?? string.Empty).Trim(), "true", StringComparison.OrdinalIgnoreCase);

            if (!checkDb)
            {
                return Json(new
                {
                    status = "ok",
                    utc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }, JsonRequestBehavior.AllowGet);
            }

            var watch = Stopwatch.StartNew();
            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    dbContext.Database.SqlQuery<int>("SELECT 1").FirstOrDefault();
                }
                watch.Stop();

                return Json(new
                {
                    status = "ok",
                    db = "ok",
                    dbMs = watch.ElapsedMilliseconds,
                    utc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                watch.Stop();
                Response.StatusCode = 503;
                return Json(new
                {
                    status = "degraded",
                    db = "error",
                    dbMs = watch.ElapsedMilliseconds,
                    utc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
