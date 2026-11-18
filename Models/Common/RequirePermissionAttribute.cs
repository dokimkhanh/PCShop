using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PCShop.Models.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public string[] PermissionCodes { get; private set; }

        public RequirePermissionAttribute(params string[] permissionCodes)
        {
            PermissionCodes = permissionCodes ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return PermissionHelper.HasPermission(httpContext.User, PermissionCodes);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var isAuthenticated = filterContext.HttpContext.User != null && filterContext.HttpContext.User.Identity.IsAuthenticated;

            if (request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.StatusCode = isAuthenticated ? 403 : 401;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        success = false,
                        Success = false,
                        message = isAuthenticated
                            ? "Bạn không có quyền thực hiện thao tác này."
                            : "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            if (!isAuthenticated)
            {
                var returnUrl = request.RawUrl;
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "Admin",
                    controller = "Account",
                    action = "Login",
                    returnUrl = returnUrl
                }));
                return;
            }

            filterContext.HttpContext.Response.StatusCode = 403;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            var viewResult = new ViewResult
            {
                ViewName = "~/Areas/Admin/Views/Shared/AccessDenied.cshtml"
            };
            viewResult.ViewBag.RequiredPermissions = string.Join(", ", PermissionCodes);
            filterContext.Result = viewResult;
        }
    }
}
