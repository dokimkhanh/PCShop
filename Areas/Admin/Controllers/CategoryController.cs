using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("Category.View", "Category.Manage")]
        public ActionResult Index(string txtSearch, int? page)
        {
            var pageSize = 10;
            var pageIndex = page ?? 1;

            IQueryable<Category> query = dbContext.categories.AsNoTracking().OrderByDescending(x => x.id);
            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                query = query.Where(x => x.title.ToLower().Contains(keyword) || x.alias.ToLower().Contains(keyword));
            }

            var categoryList = query.ToPagedList(pageIndex, pageSize);
            ViewBag._pageSize = pageSize;
            ViewBag._page = pageIndex;

            return View(categoryList);
        }

        [RequirePermission("Category.Manage")]
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
        [RequirePermission("Category.Manage")]
        public ActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                category.createdDate = DateTime.Now;
                category.createdBy = CurrentUserName();
                category.modifiedDate = DateTime.Now;
                category.alias = CommonHelper.FilterChar(category.title);
                dbContext.categories.Add(category);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [RequirePermission("Category.Manage")]
        public ActionResult Edit(int id)
        {
            var item = dbContext.categories.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Category.Manage")]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                var existing = dbContext.categories.FirstOrDefault(i => i.id == category.id);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.title = category.title;
                existing.description = category.description;
                existing.alias = CommonHelper.FilterChar(category.title);
                existing.modifiedDate = DateTime.Now;
                existing.modifiedBy = CurrentUserName();
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Category.Manage")]
        public ActionResult Delete(int id)
        {
            var item = dbContext.categories.Find(id);
            if (item == null)
            {
                return Json(new { success = false });
            }

            if (dbContext.products.Any(x => x.categoryID == id))
            {
                return Json(new { success = false, msg = "Danh mục đang có sản phẩm, không thể xoá." });
            }

            dbContext.categories.Remove(item);
            dbContext.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Category.Manage")]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var hasError = false;
                foreach (var raw in ids.Split(','))
                {
                    int id;
                    if (!int.TryParse(raw, out id))
                    {
                        continue;
                    }

                    var res = dbContext.categories.Find(id);
                    if (res == null)
                    {
                        continue;
                    }

                    if (dbContext.products.Any(x => x.categoryID == id))
                    {
                        hasError = true;
                        continue;
                    }

                    dbContext.categories.Remove(res);
                }

                dbContext.SaveChanges();
                return Json(new { success = !hasError, msg = hasError ? "Một số danh mục đang có sản phẩm nên không thể xoá." : null });
            }
            return Json(new { success = false });
        }
    }
}
