using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class PaymentMethodController : Controller
    {
        private const string PaymentGroupName = "Thanh toán";
        private ApplicationDbContext _dbContext = new ApplicationDbContext();

        [RequirePermission("Setting.View", "Setting.Manage")]
        public ActionResult Index()
        {
            var methods = PaymentConfigHelper.GetAllMethods();
            var vnpayConfig = PaymentConfigHelper.GetVnpayConfig();

            ViewBag.VnpayConfig = vnpayConfig;
            ViewBag.CanManage = PermissionHelper.HasPermission(User, "Setting.Manage");
            return View(methods);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Setting.Manage")]
        public ActionResult Toggle(int code, bool enabled)
        {
            var def = PaymentConfigHelper.GetMethodDefinition(code);
            if (def == null)
            {
                return Json(new { success = false, message = "Phương thức thanh toán không tồn tại." });
            }

            if (enabled && def.IsOnline)
            {
                var vnpayConfig = PaymentConfigHelper.GetVnpayConfig();
                if (!vnpayConfig.IsConfigured)
                {
                    return Json(new
                    {
                        success = false,
                        requireVnpayConfig = true,
                        message = "Cần điền đủ TmnCode, HashSecret và ReturnUrl của VNPAY trước khi bật kênh thanh toán trực tuyến."
                    });
                }
            }

            UpsertSetting(def.EnabledKey, def.DefaultTitle, enabled ? "true" : "false", "bool", def.DefaultDescription);
            _dbContext.SaveChanges();
            SettingsHelper.ClearAllCache();

            var methods = PaymentConfigHelper.GetAllMethods();
            var enabledCount = methods.Count(x => x.IsEnabled);

            return Json(new
            {
                success = true,
                enabled = enabled,
                enabledCount = enabledCount,
                totalCount = methods.Count,
                message = enabled ? "Đã bật " + def.DefaultTitle : "Đã tắt " + def.DefaultTitle
            });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Setting.Manage")]
        public ActionResult UpdateMethod(int code, string title, string description)
        {
            var def = PaymentConfigHelper.GetMethodDefinition(code);
            if (def == null)
            {
                return Json(new { success = false, message = "Phương thức thanh toán không tồn tại." });
            }

            var cleanTitle = (title ?? string.Empty).Trim();
            var cleanDesc = (description ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(cleanTitle))
            {
                return Json(new { success = false, message = "Tên hiển thị không được để trống." });
            }

            if (cleanTitle.Length > 120)
            {
                return Json(new { success = false, message = "Tên hiển thị tối đa 120 ký tự." });
            }

            if (cleanDesc.Length > 300)
            {
                return Json(new { success = false, message = "Mô tả hướng dẫn tối đa 300 ký tự." });
            }

            UpsertSetting(def.TitleKey, "Tên hiển thị: " + def.DefaultTitle, cleanTitle, "text", "Tiêu đề hiển thị trên trang Thanh toán.");
            UpsertSetting(def.DescriptionKey, "Mô tả: " + def.DefaultTitle, cleanDesc, "text", "Hướng dẫn ngắn hiển thị dưới tên phương thức thanh toán.");
            _dbContext.SaveChanges();
            SettingsHelper.ClearAllCache();

            return Json(new
            {
                success = true,
                title = cleanTitle,
                description = string.IsNullOrWhiteSpace(cleanDesc) ? def.DefaultDescription : cleanDesc,
                message = "Đã cập nhật thông tin hiển thị."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Setting.Manage")]
        public ActionResult SaveVnpayConfig(string environment, string vnpUrl, string tmnCode, string hashSecret, string returnUrl)
        {
            var cleanEnv = string.Equals((environment ?? string.Empty).Trim(), "production", StringComparison.OrdinalIgnoreCase)
                ? "production"
                : "sandbox";
            var cleanUrl = (vnpUrl ?? string.Empty).Trim();
            var cleanTmn = (tmnCode ?? string.Empty).Trim();
            var cleanSecret = (hashSecret ?? string.Empty).Trim();
            var cleanReturn = (returnUrl ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(cleanUrl))
            {
                cleanUrl = cleanEnv == "production"
                    ? PaymentConfigHelper.DefaultProductionUrl
                    : PaymentConfigHelper.DefaultSandboxUrl;
            }

            UpsertSetting(PaymentConfigHelper.KeyVnpayEnvironment, "Môi trường VNPAY", cleanEnv, "text", "Chế độ hoạt động sandbox hoặc production của cổng VNPAY.");
            UpsertSetting(PaymentConfigHelper.KeyVnpayUrl, "VNPAY Payment URL", cleanUrl, "text", "Địa chỉ cổng thanh toán VNPAY.");
            UpsertSetting(PaymentConfigHelper.KeyVnpayTmnCode, "VNPAY TmnCode", cleanTmn, "text", "Mã website đối tác do VNPAY cấp.");
            UpsertSetting(PaymentConfigHelper.KeyVnpayReturnUrl, "VNPAY Return URL", cleanReturn, "text", "Đường dẫn nhận kết quả trả về sau khi khách thanh toán.");

            if (!string.IsNullOrWhiteSpace(cleanSecret))
            {
                UpsertSetting(PaymentConfigHelper.KeyVnpayHashSecret, "VNPAY HashSecret", cleanSecret, "password", "Chuỗi bí mật ký kiểm tra giao dịch VNPAY.");
            }

            _dbContext.SaveChanges();
            SettingsHelper.ClearAllCache();

            TempData["Success"] = "Đã lưu cấu hình kết nối cổng VNPAY.";
            return RedirectToAction("Index");
        }

        private void UpsertSetting(string key, string name, string value, string type, string description)
        {
            var item = _dbContext.systemSettings.FirstOrDefault(x => x.settingKey == key);
            if (item == null)
            {
                _dbContext.systemSettings.Add(new SystemSettings
                {
                    settingKey = key,
                    settingName = name,
                    settingValue = value ?? string.Empty,
                    settingType = type,
                    settingGroup = PaymentGroupName,
                    settingDescription = description
                });
            }
            else
            {
                item.settingValue = value ?? string.Empty;
                item.settingGroup = PaymentGroupName;
                if (string.IsNullOrWhiteSpace(item.settingName))
                {
                    item.settingName = name;
                }
                if (string.IsNullOrWhiteSpace(item.settingType))
                {
                    item.settingType = type;
                }
            }
        }
    }
}
