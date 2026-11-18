using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class AdvController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("Setting.View", "Setting.Manage")]
        public ActionResult Index()
        {
            var banners = dbContext.advs
                .AsNoTracking()
                .OrderBy(x => x.id)
                .ToList();
            ViewBag.CanManage = PermissionHelper.HasPermission(User, "Setting.Manage");
            return View(banners);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Setting.Manage")]
        public ActionResult Save(int id, string title, string description, string image, string link, bool? isActive)
        {
            var cleanImage = (image ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanImage))
            {
                TempData["Error"] = "Vui lòng tải lên hoặc nhập đường dẫn hình ảnh banner.";
                return RedirectToAction("Index");
            }

            var cleanTitle = (title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanTitle))
            {
                cleanTitle = "Banner PCShop";
            }
            var cleanDesc = (description ?? string.Empty).Trim();
            var cleanLink = (link ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanLink))
            {
                cleanLink = "/san-pham";
            }

            var activeFlag = isActive.HasValue && isActive.Value ? 1 : 0;
            var userName = User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name)
                ? User.Identity.Name
                : "admin";
            var now = DateTime.Now;

            if (id > 0)
            {
                var existing = dbContext.advs.FirstOrDefault(x => x.id == id);
                if (existing == null)
                {
                    TempData["Error"] = "Không tìm thấy banner cần cập nhật.";
                    return RedirectToAction("Index");
                }
                existing.title = cleanTitle;
                existing.description = cleanDesc;
                existing.image = cleanImage;
                existing.link = cleanLink;
                existing.type = activeFlag;
                existing.modifiedDate = now;
                existing.modifiedBy = userName;
                TempData["Success"] = "Đã cập nhật banner thành công.";
            }
            else
            {
                dbContext.advs.Add(new Adv
                {
                    title = cleanTitle,
                    description = cleanDesc,
                    image = cleanImage,
                    link = cleanLink,
                    type = activeFlag,
                    createdDate = now,
                    createdBy = userName,
                    modifiedDate = now,
                    modifiedBy = userName
                });
                TempData["Success"] = "Đã thêm banner mới thành công.";
            }

            dbContext.SaveChanges();
            BannerHelper.ClearCache();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Setting.Manage")]
        public ActionResult Toggle(int id, bool active)
        {
            var item = dbContext.advs.FirstOrDefault(x => x.id == id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy banner." });
            }

            item.type = active ? 1 : 0;
            item.modifiedDate = DateTime.Now;
            item.modifiedBy = User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name)
                ? User.Identity.Name
                : "admin";
            dbContext.SaveChanges();
            BannerHelper.ClearCache();

            return Json(new { success = true, active = active });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Setting.Manage")]
        public ActionResult Delete(int id)
        {
            var item = dbContext.advs.FirstOrDefault(x => x.id == id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy banner." });
            }

            dbContext.advs.Remove(item);
            dbContext.SaveChanges();
            BannerHelper.ClearCache();

            return Json(new { success = true });
        }
    }
}
