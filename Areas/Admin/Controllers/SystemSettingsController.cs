using PCShop.Models;
using PCShop.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class SystemSettingsController : Controller
    {
        private ApplicationDbContext _dbContext = new ApplicationDbContext();

        private static readonly string[] OrderedGroups =
        {
            "Thông tin chung",
            "Liên hệ",
            "Mạng xã hội",
            "Giao diện",
            "SEO",
            "Khác"
        };

        [RequirePermission("Setting.View", "Setting.Manage")]
        public ActionResult Index(string group, string txtSearch)
        {
            var allSettings = _dbContext.systemSettings
                .Where(s => s.settingGroup == null || s.settingGroup != "Thanh toán")
                .OrderBy(s => s.id)
                .ToList();

            var groupCounts = allSettings
                .GroupBy(s => string.IsNullOrWhiteSpace(s.settingGroup) ? "Thông tin chung" : s.settingGroup)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var orderedGroupList = OrderedGroups
                .Where(g => groupCounts.ContainsKey(g))
                .Concat(groupCounts.Keys.Where(k => !OrderedGroups.Contains(k, StringComparer.OrdinalIgnoreCase)).OrderBy(k => k))
                .ToList();

            var filtered = allSettings.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(group))
            {
                filtered = filtered.Where(s =>
                {
                    var grp = string.IsNullOrWhiteSpace(s.settingGroup) ? "Thông tin chung" : s.settingGroup;
                    return string.Equals(grp, group, StringComparison.OrdinalIgnoreCase);
                });
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                filtered = filtered.Where(s =>
                    (s.settingName != null && s.settingName.ToLower().Contains(keyword)) ||
                    (s.settingKey != null && s.settingKey.ToLower().Contains(keyword)) ||
                    (s.settingValue != null && s.settingValue.ToLower().Contains(keyword)) ||
                    (s.settingDescription != null && s.settingDescription.ToLower().Contains(keyword)) ||
                    (s.settingGroup != null && s.settingGroup.ToLower().Contains(keyword)));
            }

            ViewBag.CurrentGroup = group;
            ViewBag.CurrentSearch = txtSearch;
            ViewBag.Groups = orderedGroupList;
            ViewBag.GroupCounts = groupCounts;
            ViewBag.TotalCount = allSettings.Count;

            return View(filtered.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Setting.Manage")]
        public ActionResult SaveAll(string returnGroup, string returnSearch)
        {
            var allSettings = _dbContext.systemSettings.ToList();
            var updatedKeys = new List<string>();

            foreach (var item in allSettings)
            {
                var fieldName = "setting_" + item.id;
                if (Request.Form.AllKeys.Contains(fieldName))
                {
                    var rawValue = Request.Form[fieldName] ?? string.Empty;
                    var newValue = string.Equals(item.settingType, "bool", StringComparison.OrdinalIgnoreCase)
                        ? (rawValue.IndexOf("true", StringComparison.OrdinalIgnoreCase) >= 0 ? "true" : "false")
                        : rawValue;

                    if (!string.Equals(item.settingValue ?? string.Empty, newValue, StringComparison.Ordinal))
                    {
                        item.settingValue = newValue;
                        updatedKeys.Add(item.settingKey);
                    }
                }
            }

            if (updatedKeys.Count > 0)
            {
                _dbContext.SaveChanges();
                foreach (var key in updatedKeys)
                {
                    SettingsHelper.ClearCache(key);
                }
            }

            TempData["Success"] = "Đã lưu cấu hình hệ thống thành công.";
            return RedirectToAction("Index", new { group = returnGroup, txtSearch = returnSearch });
        }
    }
}
