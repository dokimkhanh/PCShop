using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]

    public class NewsController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        // GET: Admin/News
        public ActionResult Index(string txtSearch, int? page)
        {
            int pageSize = 5;
            if (page == null)
            {
                page = 1;
            }

            IEnumerable<News> newsList = dbContext.news.OrderByDescending(x => x.id);
            if (!string.IsNullOrEmpty(txtSearch))
            {
                newsList = newsList.Where(x => x.title.ToLower().Contains(txtSearch) || x.alias.ToLower().Contains(txtSearch));
            }

            int pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            newsList = newsList.ToPagedList(pageIndex, pageSize);
            ViewBag._pageSize = pageSize;
            ViewBag._page = page;

            return View(newsList);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(News news)
        {
            if (ModelState.IsValid)
            {
                news.createdDate = DateTime.Now;
                news.createdBy = "Khánh tạo";
                news.modifiedDate = DateTime.Now;
                news.modifiedBy = "Khánh sửa";
                news.alias = CommonHelper.FilterChar(news.title);
                dbContext.news.Add(news);
                dbContext.SaveChanges();
                return RedirectToAction("Index");

            }
            return View(news);
        }

        public ActionResult Edit(int id)
        {
            var item = dbContext.news.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News news)
        {
            if (ModelState.IsValid)
            {
                news.modifiedBy = "Khánh sửa";

                News n = dbContext.news.Where(i => i.id == news.id).FirstOrDefault();
                n.title = news.title;
                n.description = news.description;
                n.detail = news.detail;
                n.image = news.image;
                n.modifiedDate = DateTime.Now;
                n.modifiedBy = news.modifiedBy;
                n.alias = CommonHelper.FilterChar(news.title);
                dbContext.SaveChanges();
                return RedirectToAction("Index");

            }
            return View(news);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = dbContext.news.Find(id);
            if (item != null)
            {
                var delete = dbContext.news.Remove(item);
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var id = ids.Split(',');
                if (id != null && id.Any())
                {
                    foreach (var item in id)
                    {
                        var res = dbContext.news.Find(Convert.ToInt32(item));
                        if (res != null)
                        {
                            dbContext.news.Remove(res);
                            dbContext.SaveChanges();
                        }
                    }
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false });
        }
    }
}