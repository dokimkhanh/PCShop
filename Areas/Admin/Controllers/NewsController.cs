using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("News.View", "News.Manage")]
        public ActionResult Index(string txtSearch, int? page)
        {
            var pageSize = 10;
            var pageIndex = page ?? 1;

            IQueryable<News> query = dbContext.news.AsNoTracking().OrderByDescending(x => x.id);
            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                query = query.Where(x => x.title.ToLower().Contains(keyword) || x.alias.ToLower().Contains(keyword));
            }

            var newsList = query.ToPagedList(pageIndex, pageSize);
            ViewBag._pageSize = pageSize;
            ViewBag._page = pageIndex;

            return View(newsList);
        }

        [RequirePermission("News.Manage")]
        public ActionResult Add()
        {
            return View();
        }

        private string CurrentUserName()
        {
            return User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name)
                ? User.Identity.Name
                : "system";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("News.Manage")]
        public ActionResult Add(News news)
        {
            if (ModelState.IsValid)
            {
                news.createdDate = DateTime.Now;
                news.createdBy = CurrentUserName();
                news.modifiedDate = DateTime.Now;
                news.modifiedBy = CurrentUserName();
                news.alias = CommonHelper.FilterChar(news.title);
                news.description = HtmlSecurityHelper.Sanitize(news.description);
                news.detail = HtmlSecurityHelper.Sanitize(news.detail);
                dbContext.news.Add(news);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        [RequirePermission("News.Manage")]
        public ActionResult Edit(int id)
        {
            var item = dbContext.news.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("News.Manage")]
        public ActionResult Edit(News news)
        {
            if (ModelState.IsValid)
            {
                var existing = dbContext.news.FirstOrDefault(i => i.id == news.id);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.title = news.title;
                existing.detail = HtmlSecurityHelper.Sanitize(news.detail);
                existing.description = HtmlSecurityHelper.Sanitize(news.description);
                existing.image = news.image;
                existing.modifiedDate = DateTime.Now;
                existing.modifiedBy = CurrentUserName();
                existing.alias = CommonHelper.FilterChar(news.title);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("News.Manage")]
        public ActionResult Delete(int id)
        {
            var item = dbContext.news.Find(id);
            if (item != null)
            {
                dbContext.news.Remove(item);
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("News.Manage")]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                foreach (var raw in ids.Split(','))
                {
                    int id;
                    if (!int.TryParse(raw, out id))
                    {
                        continue;
                    }

                    var res = dbContext.news.Find(id);
                    if (res != null)
                    {
                        dbContext.news.Remove(res);
                    }
                }
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
