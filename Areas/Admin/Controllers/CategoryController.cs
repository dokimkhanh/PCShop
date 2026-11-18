using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
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
            var pageSize = 15;
            var pageIndex = page ?? 1;

            var allCats = dbContext.categories.AsNoTracking().ToList();
            ViewBag.ParentNames = allCats.ToDictionary(x => x.id, x => x.title);

            IQueryable<Category> query = dbContext.categories
                .AsNoTracking()
                .OrderBy(x => x.parentID ?? x.id)
                .ThenBy(x => x.parentID.HasValue ? 1 : 0)
                .ThenBy(x => x.id);

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
            PopulateParentDropdown(null, null);
            return View();
        }

        private string CurrentUserName()
        {
            return User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name)
                ? User.Identity.Name
                : "system";
        }

        private void PopulateParentDropdown(int? excludeId, int? selectedParentId)
        {
            var parents = dbContext.categories
                .AsNoTracking()
                .Where(x => x.parentID == null && (!excludeId.HasValue || x.id != excludeId.Value))
                .OrderBy(x => x.id)
                .ToList();
            ViewBag.ParentCategories = new SelectList(parents, "id", "title", selectedParentId);
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
                CategoryMenuHelper.ClearCache();
                return RedirectToAction("Index");
            }
            PopulateParentDropdown(null, category.parentID);
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
            PopulateParentDropdown(item.id, item.parentID);
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
                existing.parentID = category.parentID == existing.id ? null : category.parentID;
                existing.alias = CommonHelper.FilterChar(category.title);
                existing.modifiedDate = DateTime.Now;
                existing.modifiedBy = CurrentUserName();
                dbContext.SaveChanges();
                CategoryMenuHelper.ClearCache();
                return RedirectToAction("Index");
            }
            PopulateParentDropdown(category.id, category.parentID);
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

            if (dbContext.categories.Any(x => x.parentID == id))
            {
                return Json(new { success = false, msg = "Danh mục đang có danh mục con, không thể xoá." });
            }

            if (dbContext.products.Any(x => x.categoryID == id))
            {
                return Json(new { success = false, msg = "Danh mục đang có sản phẩm, không thể xoá." });
            }

            dbContext.categories.Remove(item);
            dbContext.SaveChanges();
            CategoryMenuHelper.ClearCache();
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

                    if (dbContext.products.Any(x => x.categoryID == id) || dbContext.categories.Any(x => x.parentID == id))
                    {
                        hasError = true;
                        continue;
                    }

                    dbContext.categories.Remove(res);
                }

                dbContext.SaveChanges();
                CategoryMenuHelper.ClearCache();
                return Json(new { success = !hasError, msg = hasError ? "Một số danh mục đang có danh mục con hoặc sản phẩm nên không thể xoá." : null });
            }
            return Json(new { success = false });
        }
    }
}
