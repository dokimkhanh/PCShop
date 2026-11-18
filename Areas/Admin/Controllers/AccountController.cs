using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PCShop.Models;
using PCShop.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        [RequirePermission("Account.View", "Account.Manage")]
        public ActionResult Index(string txtSearch)
        {
            var roleMap = _dbContext.Roles.ToDictionary(r => r.Id, r => r.Name);
            var users = _dbContext.Users.OrderBy(u => u.UserName).ToList();

            var items = users.Select(u => new AdminUserListItemViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Phone = u.Phone,
                Email = u.Email,
                Roles = u.Roles
                    .Select(ur => roleMap.ContainsKey(ur.RoleId) ? roleMap[ur.RoleId] : "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToList()
            }).ToList();

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                items = items.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(keyword)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(keyword)) ||
                    (u.Roles != null && u.Roles.Any(r => r.ToLower().Contains(keyword)))
                ).ToList();
            }

            ViewBag.CurrentSearch = txtSearch;
            return View(items);
        }

        [RequirePermission("Account.Manage")]
        public ActionResult Register()
        {
            var model = new AdminRegisterViewModel
            {
                SelectedRoles = new List<string>(),
                AvailableRoles = _dbContext.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Account.Manage")]
        public async Task<ActionResult> Register(AdminRegisterViewModel model)
        {
            var allRoleNames = _dbContext.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToList();
            model.AvailableRoles = allRoleNames;
            model.SelectedRoles = (model.SelectedRoles ?? new List<string>())
                .Where(r => allRoleNames.Contains(r))
                .Distinct()
                .ToList();

            if (model.SelectedRoles.Count == 0)
            {
                ModelState.AddModelError("SelectedRoles", "Vui lòng chọn ít nhất một nhóm quyền.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleResult = await UserManager.AddToRolesAsync(user.Id, model.SelectedRoles.ToArray());
                    if (roleResult.Succeeded)
                    {
                        PermissionHelper.ClearUserCache(user.Id);
                        TempData["Success"] = "Thêm tài khoản thành công.";
                        return RedirectToAction("Index", "Account");
                    }

                    await UserManager.DeleteAsync(user);
                    AddErrors(roleResult);
                }
                else
                {
                    AddErrors(result);
                }
            }

            return View(model);
        }

        [RequirePermission("Account.Manage")]
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            var user = await UserManager.FindByIdAsync(id) ?? await UserManager.FindByNameAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var allRoleNames = _dbContext.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToList();
            var userRoles = (await UserManager.GetRolesAsync(user.Id)).ToList();

            var model = new EditAccountViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                SelectedRoles = userRoles,
                AvailableRoles = allRoleNames
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Account.Manage")]
        public async Task<ActionResult> Edit(EditAccountViewModel model)
        {
            var allRoleNames = _dbContext.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToList();
            model.AvailableRoles = allRoleNames;
            model.SelectedRoles = (model.SelectedRoles ?? new List<string>())
                .Where(r => allRoleNames.Contains(r))
                .Distinct()
                .ToList();

            if (model.SelectedRoles.Count == 0)
            {
                ModelState.AddModelError("SelectedRoles", "Vui lòng chọn ít nhất một nhóm quyền.");
            }

            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.Phone;

                var updateResult = await UserManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    AddErrors(updateResult);
                    return View(model);
                }

                var currentRoles = await UserManager.GetRolesAsync(user.Id);
                var toRemove = currentRoles.Except(model.SelectedRoles).ToArray();
                var toAdd = model.SelectedRoles.Except(currentRoles).ToArray();

                if (toRemove.Length > 0)
                {
                    await UserManager.RemoveFromRolesAsync(user.Id, toRemove);
                }
                if (toAdd.Length > 0)
                {
                    await UserManager.AddToRolesAsync(user.Id, toAdd);
                }

                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var pwdResult = await UserManager.ResetPasswordAsync(user.Id, resetToken, model.NewPassword);
                    if (!pwdResult.Succeeded)
                    {
                        AddErrors(pwdResult);
                        return View(model);
                    }
                }

                PermissionHelper.ClearUserCache(user.Id);
                TempData["Success"] = "Cập nhật tài khoản và nhóm quyền thành công.";
                return RedirectToAction("Index", "Account");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Account.Manage")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var currentUserId = User.Identity.GetUserId();
            if (string.Equals(currentUserId, id, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Không thể xoá tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    PermissionHelper.ClearUserCache(id);
                    TempData["Success"] = "Đã xoá tài khoản.";
                }
                else
                {
                    TempData["Error"] = string.Join("; ", result.Errors);
                }
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    var signedInUser = UserManager.FindByName(model.UserName);
                    if (signedInUser != null)
                    {
                        PermissionHelper.ClearUserCache(signedInUser.Id);
                    }
                    if (signedInUser == null || (!UserManager.IsInRole(signedInUser.Id, "Admin") && !PermissionHelper.HasAnyAdminPermission(signedInUser.Id)))
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        ModelState.AddModelError("", "Tài khoản không có quyền truy cập trang quản trị.");
                        return View(model);
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var userId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                PermissionHelper.ClearUserCache(userId);
            }
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
