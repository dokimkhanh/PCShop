using System;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PCShop.Models.Common
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAntiForgeryTokenAjaxAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");
            var request = filterContext.HttpContext.Request;
            if (!string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) return;

            var cookie = request.Cookies[AntiForgeryConfig.CookieName];
            var formToken = request.Form["__RequestVerificationToken"];
            if (string.IsNullOrEmpty(formToken)) formToken = request.Headers["__RequestVerificationToken"];

            try
            {
                AntiForgery.Validate(cookie != null ? cookie.Value : null, formToken);
            }
            catch (HttpAntiForgeryException)
            {
                filterContext.Result = new HttpStatusCodeResult(400, "Invalid anti-forgery token.");
            }
        }
    }
}
