using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PCShop.Models.Common
{
    public class MailHelper
    {
        private const int RequestTimeoutSeconds = 30;
        private const string DefaultApiUrl = "https://api.brevo.com/v3/smtp/email";

        private static readonly HttpClient Client = CreateHttpClient();

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);
            return client;
        }

        public static bool SendMail(string name, string subject, string content, string toMail)
        {
            try
            {
                return SendMailAsync(name, subject, content, toMail).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Trace.TraceError("MailHelper.SendMail error: " + ex);
                return false;
            }
        }

        public static async Task<bool> SendMailAsync(string name, string subject, string content, string toMail)
        {
            try
            {
                if (!IsValidEmail(toMail))
                {
                    Trace.TraceError("Brevo send failed: invalid recipient email '" + (toMail ?? string.Empty) + "'");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(subject))
                {
                    Trace.TraceError("Brevo send failed: subject is empty");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    Trace.TraceError("Brevo send failed: content is empty");
                    return false;
                }

                string apiKey = GetSetting("Brevo_ApiKey");
                string fromEmail = GetSetting("Brevo_FromEmail");
                string fromName = GetSetting("Brevo_FromName");
                string apiUrl = GetSetting("Brevo_ApiUrl");

                if (string.IsNullOrEmpty(apiKey))
                {
                    Trace.TraceError("Brevo send failed: missing appSettings key 'Brevo_ApiKey'");
                    return false;
                }

                if (apiKey.StartsWith("xsmtpsib-", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceError("Brevo send failed: 'Brevo_ApiKey' is an SMTP key (xsmtpsib-...). Brevo API v3 requires an API key (xkeysib-...).");
                    return false;
                }

                if (!IsValidEmail(fromEmail))
                {
                    Trace.TraceError("Brevo send failed: missing or invalid appSettings key 'Brevo_FromEmail'");
                    return false;
                }

                if (string.IsNullOrEmpty(apiUrl))
                {
                    apiUrl = DefaultApiUrl;
                }

                string senderName = !string.IsNullOrWhiteSpace(name)
                    ? name.Trim()
                    : (!string.IsNullOrWhiteSpace(fromName) ? fromName.Trim() : fromEmail);

                var payload = new
                {
                    sender = new { name = senderName, email = fromEmail },
                    to = new[] { new { email = toMail.Trim() } },
                    subject = subject,
                    htmlContent = content
                };

                string json = JsonConvert.SerializeObject(payload);

                using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
                {
                    request.Headers.TryAddWithoutValidation("api-key", apiKey);
                    request.Headers.TryAddWithoutValidation("accept", "application/json");
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await Client.SendAsync(request).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Trace.TraceError(string.Format(
                                "Brevo send failed: {0} {1}",
                                (int)response.StatusCode,
                                responseBody));
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Brevo send failed: " + ex);
                return false;
            }
        }

        private static string GetSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return value == null ? null : value.Trim();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return EmailRegex.IsMatch(email.Trim());
        }
    }
}
