using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("Order.View", "Order.Manage")]
        public ActionResult Index(int? page, int? status, string txtSearch)
        {
            var baseQuery = dbContext.orders.AsNoTracking();

            ViewBag.CountAll = baseQuery.Count();
            ViewBag.CountPending = baseQuery.Count(x => x.paymentStatus == 0);
            ViewBag.CountPaid = baseQuery.Count(x => x.paymentStatus == 1);
            ViewBag.CountCancelled = baseQuery.Count(x => x.paymentStatus == 2);

            IQueryable<PCShop.Models.EF.Order> query = baseQuery;

            if (status.HasValue && (status.Value == 0 || status.Value == 1 || status.Value == 2))
            {
                query = query.Where(x => x.paymentStatus == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                query = query.Where(x =>
                    (x.code != null && x.code.ToLower().Contains(keyword)) ||
                    (x.customerName != null && x.customerName.ToLower().Contains(keyword)) ||
                    (x.phone != null && x.phone.Contains(keyword)));
            }

            var orderedQuery = query.OrderByDescending(x => x.createdDate);

            var pageSize = 10;
            var pageNumber = page ?? 1;

            ViewBag._pageSize = pageSize;
            ViewBag._page = pageNumber;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = txtSearch;

            return View(orderedQuery.ToPagedList(pageNumber, pageSize));
        }

        [RequirePermission("Order.View", "Order.Manage")]
        public ActionResult View(int id)
        {
            var item = dbContext.orders
                .Include(x => x.orderDetails.Select(d => d.product.productImages))
                .FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Order.Manage")]
        public ActionResult UpdatePayment(int id, int status)
        {
            if (status != 0 && status != 1 && status != 2)
            {
                return Json(new { Success = false });
            }

            var item = dbContext.orders.Find(id);
            if (item != null)
            {
                item.paymentStatus = status;
                item.modifiedDate = DateTime.Now;
                dbContext.Entry(item).Property(x => x.paymentStatus).IsModified = true;
                dbContext.Entry(item).Property(x => x.modifiedDate).IsModified = true;
                dbContext.SaveChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
    }
}
