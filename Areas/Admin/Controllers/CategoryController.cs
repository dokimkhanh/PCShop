using PCShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PCShop.Models.EF;
using PagedList;
using System.Web.UI;
using PCShop.Models.Common;

namespace PCShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Nhân viên")]


    public class CategoryController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Admin/Category
        public ActionResult Index(string txtSearch, int? page)
        {
            int pageSize = 5;
            if (page == null)
            {
                page = 1;
            }

            IEnumerable<Category> categoryList = dbContext.categories.OrderByDescending(x => x.id);
            if (!string.IsNullOrEmpty(txtSearch))
            {
                categoryList = categoryList.Where(x => x.title.ToLower().Contains(txtSearch) || x.alias.ToLower().Contains(txtSearch));
            }

            int pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            categoryList = categoryList.ToPagedList(pageIndex, pageSize);
            ViewBag._pageSize = pageSize;
            ViewBag._page = page;

            return View(categoryList);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                category.createdDate = DateTime.Now;
                category.createdBy = "Khánh";
                category.modifiedDate = DateTime.Now;
                category.alias = CommonHelper.FilterChar(category.title);
                dbContext.categories.Add(category);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public ActionResult Edit(int id)
        {
            var item = dbContext.categories.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                category.modifiedDate = DateTime.Now;
                category.modifiedBy = "Khánh sửa";

                Category c = dbContext.categories.Where(i => i.id == category.id).FirstOrDefault();
                c.title = category.title;
                c.description = category.description;
                c.modifiedDate = category.modifiedDate;
                c.modifiedBy = category.modifiedBy;
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = dbContext.categories.Find(id);
            if (item != null)
            {
                var delete = dbContext.categories.Remove(item);
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
                        var res = dbContext.categories.Find(Convert.ToInt32(item));
                        if (res != null)
                        {
                            dbContext.categories.Remove(res);
                            dbContext.SaveChanges();
                        }
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}