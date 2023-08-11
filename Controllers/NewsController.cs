using PagedList;
using PCShop.Models;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext _dbContext = new ApplicationDbContext();
        // GET: News
        public ActionResult Index(int? page)
        {
            int pageSize = 10;
            if (page == null)
            {
                page = 1;
            }

            IEnumerable<News> newsList = _dbContext.news.OrderByDescending(x => x.id);
            int pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            newsList = newsList.ToPagedList(pageIndex, pageSize);

            return View(newsList);
        }


        public ActionResult NewsDetail(int id) {
            var _new = _dbContext.news.Find(id);
            ViewBag.Tieude = _new.title;
            return View(_new);
        }
        public ActionResult Partial_News()
        {
            var news = _dbContext.news.Take(3).ToList();
            return PartialView(news);
        }
    }
}