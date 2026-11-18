using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class RoleController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("Role.View", "Role.Manage")]
        public ActionResult Index(string tab, string txtSearch)
        {
            var activeTab = string.Equals(tab, "permissions", StringComparison.OrdinalIgnoreCase) ? "permissions" : "groups";
            var allPermissions = dbContext.permissions
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Code)
                .ToList();

            var rolePermCounts = dbContext.rolePermissions
                .GroupBy(rp => rp.RoleId)
                .ToDictionary(g => g.Key, g => g.Count());

            var roles = dbContext.Roles.OrderBy(r => r.Name).ToList();
            var roleItems = roles.Select(r => new RoleGroupItemViewModel
            {
                Id = r.Id,
                Name = r.Name,
                PermissionCount = string.Equals(r.Name, "Admin", StringComparison.OrdinalIgnoreCase)
                    ? allPermissions.Count
                    : (rolePermCounts.ContainsKey(r.Id) ? rolePermCounts[r.Id] : 0),
                TotalPermissions = allPermissions.Count,
                UserCount = dbContext.Users.Count(u => u.Roles.Any(ur => ur.RoleId == r.Id))
            }).ToList();

            var filteredRoles = roleItems;
            var filteredPermissions = allPermissions;

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                filteredRoles = roleItems
                    .Where(r => r.Name != null && r.Name.ToLower().Contains(keyword))
                    .ToList();
                filteredPermissions = allPermissions
                    .Where(p =>
                        (p.Name != null && p.Name.ToLower().Contains(keyword)) ||
                        (p.Code != null && p.Code.ToLower().Contains(keyword)) ||
                        (p.Module != null && p.Module.ToLower().Contains(keyword)) ||
                        (p.Description != null && p.Description.ToLower().Contains(keyword)))
                    .ToList();
            }

            ViewBag.CurrentSearch = txtSearch;
            ViewBag.TotalRoleCount = roleItems.Count;
            ViewBag.TotalPermissionCount = allPermissions.Count;

            var model = new RoleManagementViewModel
            {
                ActiveTab = activeTab,
                Roles = filteredRoles,
                Permissions = filteredPermissions
            };

            return View(model);
        }

        [RequirePermission("Role.Manage")]
        public ActionResult Add()
        {
            var model = new RoleFormViewModel
            {
                SelectedPermissionIds = new List<int>(),
                AllPermissions = dbContext.permissions.OrderBy(p => p.Module).ThenBy(p => p.Code).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Role.Manage")]
        public ActionResult Add(RoleFormViewModel model)
        {
            model.AllPermissions = dbContext.permissions.OrderBy(p => p.Module).ThenBy(p => p.Code).ToList();
            model.SelectedPermissionIds = model.SelectedPermissionIds ?? new List<int>();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleName = (model.Name ?? "").Trim();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
            if (roleManager.RoleExists(roleName))
            {
                ModelState.AddModelError("Name", "Nhóm quyền này đã tồn tại.");
                return View(model);
            }

            var newRole = new IdentityRole(roleName);
            var result = roleManager.Create(newRole);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(model);
            }

            var validIds = model.AllPermissions.Select(p => p.Id).ToHashSet();
            foreach (var permId in model.SelectedPermissionIds.Distinct().Where(id => validIds.Contains(id)))
            {
                dbContext.rolePermissions.Add(new RolePermission
                {
                    RoleId = newRole.Id,
                    PermissionId = permId
                });
            }
            dbContext.SaveChanges();
            PermissionHelper.ClearAllCache();

            TempData["Success"] = "Thêm nhóm quyền thành công.";
            return RedirectToAction("Index");
        }

        [RequirePermission("Role.Manage")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            var role = dbContext.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }

            var allPermissions = dbContext.permissions.OrderBy(p => p.Module).ThenBy(p => p.Code).ToList();
            var selectedIds = string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase)
                ? allPermissions.Select(p => p.Id).ToList()
                : dbContext.rolePermissions.Where(rp => rp.RoleId == role.Id).Select(rp => rp.PermissionId).ToList();

            var model = new RoleFormViewModel
            {
                Id = role.Id,
                Name = role.Name,
                SelectedPermissionIds = selectedIds,
                AllPermissions = allPermissions
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Role.Manage")]
        public ActionResult Edit(RoleFormViewModel model)
        {
            model.AllPermissions = dbContext.permissions.OrderBy(p => p.Module).ThenBy(p => p.Code).ToList();
            model.SelectedPermissionIds = model.SelectedPermissionIds ?? new List<int>();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = dbContext.Roles.Find(model.Id);
            if (role == null)
            {
                return HttpNotFound();
            }

            var isAdminRole = string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase);
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
            var newName = (model.Name ?? "").Trim();

            if (!isAdminRole)
            {
                var existing = roleManager.FindByName(newName);
                if (existing != null && existing.Id != role.Id)
                {
                    ModelState.AddModelError("Name", "Tên nhóm quyền này đã tồn tại.");
                    return View(model);
                }

                role.Name = newName;
                var updateResult = roleManager.Update(role);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View(model);
                }
            }

            var oldMappings = dbContext.rolePermissions.Where(rp => rp.RoleId == role.Id).ToList();
            if (oldMappings.Any())
            {
                dbContext.rolePermissions.RemoveRange(oldMappings);
            }

            var validIds = model.AllPermissions.Select(p => p.Id).ToHashSet();
            var idsToAssign = isAdminRole
                ? validIds.ToList()
                : model.SelectedPermissionIds.Distinct().Where(id => validIds.Contains(id)).ToList();

            foreach (var permId in idsToAssign)
            {
                dbContext.rolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permId
                });
            }

            dbContext.SaveChanges();
            PermissionHelper.ClearAllCache();

            TempData["Success"] = "Cập nhật nhóm quyền và phân quyền thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Role.Manage")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Không xác định được nhóm quyền cần xoá.";
                return RedirectToAction("Index");
            }

            var role = dbContext.Roles.Find(id);
            if (role == null)
            {
                TempData["Error"] = "Nhóm quyền không tồn tại.";
                return RedirectToAction("Index");
            }

            if (string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Không thể xoá nhóm quyền Admin.";
                return RedirectToAction("Index");
            }

            var hasUser = dbContext.Users.Any(u => u.Roles.Any(r => r.RoleId == role.Id));
            if (hasUser)
            {
                TempData["Error"] = "Không thể xoá nhóm quyền đang được gán cho tài khoản.";
                return RedirectToAction("Index");
            }

            var mappings = dbContext.rolePermissions.Where(rp => rp.RoleId == role.Id).ToList();
            if (mappings.Any())
            {
                dbContext.rolePermissions.RemoveRange(mappings);
                dbContext.SaveChanges();
            }

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
            var result = roleManager.Delete(role);
            if (result.Succeeded)
            {
                PermissionHelper.ClearAllCache();
                TempData["Success"] = "Đã xoá nhóm quyền.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = string.Join("; ", result.Errors);
            return RedirectToAction("Index");
        }

        [RequirePermission("Role.Manage")]
        public ActionResult AddPermission()
        {
            return View(new Permission());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Role.Manage")]
        public ActionResult AddPermission(Permission model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Code = (model.Code ?? "").Trim();
            model.Name = (model.Name ?? "").Trim();
            model.Module = (model.Module ?? "").Trim();

            if (dbContext.permissions.Any(p => p.Code == model.Code))
            {
                ModelState.AddModelError("Code", "Mã quyền này đã tồn tại.");
                return View(model);
            }

            dbContext.permissions.Add(model);
            dbContext.SaveChanges();

            var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole != null)
            {
                dbContext.rolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = model.Id
                });
                dbContext.SaveChanges();
            }

            PermissionHelper.ClearAllCache();
            TempData["Success"] = "Thêm quyền mới thành công.";
            return RedirectToAction("Index", new { tab = "permissions" });
        }

        [RequirePermission("Role.Manage")]
        public ActionResult EditPermission(int id)
        {
            var perm = dbContext.permissions.Find(id);
            if (perm == null)
            {
                return HttpNotFound();
            }
            return View(perm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Role.Manage")]
        public ActionResult EditPermission(Permission model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = dbContext.permissions.Find(model.Id);
            if (existing == null)
            {
                return HttpNotFound();
            }

            var code = (model.Code ?? "").Trim();
            if (dbContext.permissions.Any(p => p.Code == code && p.Id != model.Id))
            {
                ModelState.AddModelError("Code", "Mã quyền này đã tồn tại.");
                return View(model);
            }

            existing.Code = code;
            existing.Name = (model.Name ?? "").Trim();
            existing.Module = (model.Module ?? "").Trim();
            existing.Description = model.Description;
            dbContext.SaveChanges();

            PermissionHelper.ClearAllCache();
            TempData["Success"] = "Cập nhật quyền thành công.";
            return RedirectToAction("Index", new { tab = "permissions" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Role.Manage")]
        public ActionResult DeletePermission(int id)
        {
            var perm = dbContext.permissions.Find(id);
            if (perm != null)
            {
                var mappings = dbContext.rolePermissions.Where(rp => rp.PermissionId == id).ToList();
                if (mappings.Any())
                {
                    dbContext.rolePermissions.RemoveRange(mappings);
                }
                dbContext.permissions.Remove(perm);
                dbContext.SaveChanges();
                PermissionHelper.ClearAllCache();
                TempData["Success"] = "Đã xoá quyền.";
            }
            return RedirectToAction("Index", new { tab = "permissions" });
        }
    }
}
