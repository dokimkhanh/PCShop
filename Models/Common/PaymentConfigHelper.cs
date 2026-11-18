using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PCShop.Models.Common
{
    public class PaymentMethodInfo
    {
        public int Code { get; set; }
        public string Group { get; set; }
        public string EnabledKey { get; set; }
        public string TitleKey { get; set; }
        public string DescriptionKey { get; set; }
        public string DefaultTitle { get; set; }
        public string DefaultDescription { get; set; }
        public bool DefaultEnabled { get; set; }
        public string IconClass { get; set; }
        public bool IsOnline { get; set; }
        public bool IsEnabled { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class VnpayConfigInfo
    {
        public string Environment { get; set; }
        public string Url { get; set; }
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string ReturnUrl { get; set; }
        public bool HasHashSecret { get; set; }
        public bool IsConfigured { get; set; }
        public bool TmnCodeFromServer { get; set; }
        public bool HashSecretFromServer { get; set; }
        public bool ReturnUrlFromServer { get; set; }
    }

    public static class PaymentConfigHelper
    {
        public const int PaymentTypeVnpayGateway = 0;
        public const int PaymentTypeVnpayQr = 1;
        public const int PaymentTypeVnpayBank = 2;
        public const int PaymentTypeVnpayCard = 3;
        public const int PaymentTypeCod = 10;

        public const string DefaultSandboxUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public const string DefaultProductionUrl = "https://pay.vnpayment.vn/vpcpay.html";

        public const string KeyVnpayEnvironment = "PaymentVnpayEnvironment";
        public const string KeyVnpayUrl = "PaymentVnpayUrl";
        public const string KeyVnpayTmnCode = "PaymentVnpayTmnCode";
        public const string KeyVnpayHashSecret = "PaymentVnpayHashSecret";
        public const string KeyVnpayReturnUrl = "PaymentVnpayReturnUrl";

        private static readonly PaymentMethodInfo[] Definitions =
        {
            new PaymentMethodInfo
            {
                Code = PaymentTypeCod,
                Group = "offline",
                EnabledKey = "PaymentCodEnabled",
                TitleKey = "PaymentCodTitle",
                DescriptionKey = "PaymentCodDescription",
                DefaultEnabled = true,
                DefaultTitle = "Thanh toán khi nhận hàng (COD)",
                DefaultDescription = "Kiểm tra sản phẩm trước khi thanh toán trực tiếp cho nhân viên giao hàng.",
                IconClass = "fas fa-money-bill-wave",
                IsOnline = false
            },
            new PaymentMethodInfo
            {
                Code = PaymentTypeVnpayGateway,
                Group = "online",
                EnabledKey = "PaymentVnpayGatewayEnabled",
                TitleKey = "PaymentVnpayGatewayTitle",
                DescriptionKey = "PaymentVnpayGatewayDescription",
                DefaultEnabled = false,
                DefaultTitle = "Cổng thanh toán VNPAY",
                DefaultDescription = "Thanh toán trực tuyến nhanh chóng qua cổng thanh toán VNPAY.",
                IconClass = "fas fa-wallet",
                IsOnline = true
            },
            new PaymentMethodInfo
            {
                Code = PaymentTypeVnpayQr,
                Group = "online",
                EnabledKey = "PaymentVnpayQrEnabled",
                TitleKey = "PaymentVnpayQrTitle",
                DescriptionKey = "PaymentVnpayQrDescription",
                DefaultEnabled = false,
                DefaultTitle = "Ứng dụng hỗ trợ VNPAYQR",
                DefaultDescription = "Quét mã QR bằng ứng dụng ngân hàng hoặc ví điện tử hỗ trợ VNPAYQR.",
                IconClass = "fas fa-qrcode",
                IsOnline = true
            },
            new PaymentMethodInfo
            {
                Code = PaymentTypeVnpayBank,
                Group = "online",
                EnabledKey = "PaymentVnpayBankEnabled",
                TitleKey = "PaymentVnpayBankTitle",
                DescriptionKey = "PaymentVnpayBankDescription",
                DefaultEnabled = false,
                DefaultTitle = "Thẻ ngân hàng nội địa (ATM)",
                DefaultDescription = "Thanh toán bằng thẻ ATM nội địa hoặc tài khoản ngân hàng có Internet Banking.",
                IconClass = "fas fa-university",
                IsOnline = true
            },
            new PaymentMethodInfo
            {
                Code = PaymentTypeVnpayCard,
                Group = "online",
                EnabledKey = "PaymentVnpayCardEnabled",
                TitleKey = "PaymentVnpayCardTitle",
                DescriptionKey = "PaymentVnpayCardDescription",
                DefaultEnabled = false,
                DefaultTitle = "Thẻ thanh toán quốc tế",
                DefaultDescription = "Thanh toán bằng thẻ Visa, Mastercard, JCB hoặc UnionPay.",
                IconClass = "fas fa-credit-card",
                IsOnline = true
            }
        };

        public static List<PaymentMethodInfo> GetAllMethods()
        {
            var result = new List<PaymentMethodInfo>(Definitions.Length);
            foreach (var def in Definitions)
            {
                var customTitle = SettingsHelper.GetValue(def.TitleKey);
                var customDesc = SettingsHelper.GetValue(def.DescriptionKey);
                result.Add(new PaymentMethodInfo
                {
                    Code = def.Code,
                    Group = def.Group,
                    EnabledKey = def.EnabledKey,
                    TitleKey = def.TitleKey,
                    DescriptionKey = def.DescriptionKey,
                    DefaultEnabled = def.DefaultEnabled,
                    DefaultTitle = def.DefaultTitle,
                    DefaultDescription = def.DefaultDescription,
                    IconClass = def.IconClass,
                    IsOnline = def.IsOnline,
                    IsEnabled = SettingsHelper.GetBool(def.EnabledKey, def.DefaultEnabled),
                    Title = string.IsNullOrWhiteSpace(customTitle) ? def.DefaultTitle : customTitle.Trim(),
                    Description = string.IsNullOrWhiteSpace(customDesc) ? def.DefaultDescription : customDesc.Trim()
                });
            }
            return result;
        }

        public static PaymentMethodInfo GetMethodDefinition(int code)
        {
            return Definitions.FirstOrDefault(x => x.Code == code);
        }

        public static bool IsMethodEnabled(int code)
        {
            var def = GetMethodDefinition(code);
            if (def == null)
            {
                return false;
            }
            return SettingsHelper.GetBool(def.EnabledKey, def.DefaultEnabled);
        }

        public static string GetMethodTitle(int code)
        {
            var def = GetMethodDefinition(code);
            if (def == null)
            {
                return "Thanh toán trực tuyến";
            }
            var customTitle = SettingsHelper.GetValue(def.TitleKey);
            return string.IsNullOrWhiteSpace(customTitle) ? def.DefaultTitle : customTitle.Trim();
        }

        public static VnpayConfigInfo GetVnpayConfig()
        {
            var dbEnv = SettingsHelper.GetValue(KeyVnpayEnvironment);
            var dbUrl = SettingsHelper.GetValue(KeyVnpayUrl);
            var dbTmn = SettingsHelper.GetValue(KeyVnpayTmnCode);
            var dbSecret = SettingsHelper.GetValue(KeyVnpayHashSecret);
            var dbReturn = SettingsHelper.GetValue(KeyVnpayReturnUrl);

            var cfgUrl = ConfigurationManager.AppSettings["vnp_Url"] ?? string.Empty;
            var cfgTmn = ConfigurationManager.AppSettings["vnp_TmnCode"] ?? string.Empty;
            var cfgSecret = ConfigurationManager.AppSettings["vnp_HashSecret"] ?? string.Empty;
            var cfgReturn = ConfigurationManager.AppSettings["vnp_Returnurl"] ?? string.Empty;

            var url = !string.IsNullOrWhiteSpace(dbUrl) ? dbUrl.Trim() : cfgUrl.Trim();
            var tmn = !string.IsNullOrWhiteSpace(dbTmn) ? dbTmn.Trim() : cfgTmn.Trim();
            var secret = !string.IsNullOrWhiteSpace(dbSecret) ? dbSecret.Trim() : cfgSecret.Trim();
            var returnUrl = !string.IsNullOrWhiteSpace(dbReturn) ? dbReturn.Trim() : cfgReturn.Trim();

            var env = !string.IsNullOrWhiteSpace(dbEnv)
                ? dbEnv.Trim().ToLowerInvariant()
                : (url.IndexOf("sandbox", StringComparison.OrdinalIgnoreCase) >= 0 || string.IsNullOrWhiteSpace(url) ? "sandbox" : "production");

            if (string.IsNullOrWhiteSpace(url))
            {
                url = env == "production" ? DefaultProductionUrl : DefaultSandboxUrl;
            }

            return new VnpayConfigInfo
            {
                Environment = env == "production" ? "production" : "sandbox",
                Url = url,
                TmnCode = tmn,
                HashSecret = secret,
                ReturnUrl = returnUrl,
                HasHashSecret = !string.IsNullOrWhiteSpace(secret),
                IsConfigured = !string.IsNullOrWhiteSpace(url)
                    && !string.IsNullOrWhiteSpace(tmn)
                    && !string.IsNullOrWhiteSpace(secret)
                    && !string.IsNullOrWhiteSpace(returnUrl),
                TmnCodeFromServer = string.IsNullOrWhiteSpace(dbTmn) && !string.IsNullOrWhiteSpace(cfgTmn),
                HashSecretFromServer = string.IsNullOrWhiteSpace(dbSecret) && !string.IsNullOrWhiteSpace(cfgSecret),
                ReturnUrlFromServer = string.IsNullOrWhiteSpace(dbReturn) && !string.IsNullOrWhiteSpace(cfgReturn)
            };
        }
    }
}
