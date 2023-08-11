using PagedList;
using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]

    public class OrderController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(int? page)
        {
            var items = dbContext.orders.OrderByDescending(x => x.createdDate).ToList();

            int pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;

            ViewBag._pageSize = pageSize;
            ViewBag._page = page;

            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult View(int id)
        {
            var item = dbContext.orders.Find(id);
            return View(item);
        }

        [HttpPost]
        public ActionResult UpdatePayment(int id, int status)
        {
            var item = dbContext.orders.Find(id);
            if (item != null)
            {
                dbContext.orders.Attach(item);
                item.paymentStatus = status;
                dbContext.Entry(item).Property(x => x.paymentStatus).IsModified = true;
                dbContext.SaveChanges();
                return Json(new { Success = true });

            }
            return Json(new { Success = false });

        }
    }
}