using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;

namespace PCShop.Models.Common
{
    public static class PermissionHelper
    {
        private const string CachePrefix = "UserPermissions_";
        private const int CacheMinutes = 2;

        public static HashSet<string> GetUserPermissions(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var cacheKey = CachePrefix + userId;
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                var cached = cache.Get(cacheKey) as HashSet<string>;
                if (cached != null)
                {
                    return cached;
                }
            }

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    var roleIds = user.Roles.Select(r => r.RoleId).ToList();
                    if (roleIds.Any())
                    {
                        var isAdmin = db.Roles.Any(r => roleIds.Contains(r.Id) && r.Name == "Admin");
                        if (isAdmin)
                        {
                            result.Add("*");
                        }
                        else
                        {
                            var codes = db.rolePermissions
                                .Where(rp => roleIds.Contains(rp.RoleId) && rp.Permission != null)
                                .Select(rp => rp.Permission.Code)
                                .Distinct()
                                .ToList();

                            foreach (var code in codes)
                            {
                                result.Add(code);
                            }
                        }
                    }
                }
            }

            if (cache != null)
            {
                cache.Insert(cacheKey, result, null, DateTime.Now.AddMinutes(CacheMinutes), Cache.NoSlidingExpiration);
            }

            return result;
        }

        public static bool HasPermission(IPrincipal principal, params string[] permissionCodes)
        {
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return false;
            }

            if (principal.IsInRole("Admin"))
            {
                return true;
            }

            var userId = principal.Identity.GetUserId();
            var userPerms = GetUserPermissions(userId);
            if (userPerms.Contains("*"))
            {
                return true;
            }

            if (permissionCodes == null || permissionCodes.Length == 0)
            {
                return userPerms.Count > 0;
            }

            return permissionCodes.Any(code => !string.IsNullOrWhiteSpace(code) && userPerms.Contains(code));
        }

        public static bool HasAnyAdminPermission(string userId)
        {
            var perms = GetUserPermissions(userId);
            return perms.Count > 0;
        }

        public static void ClearUserCache(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                cache.Remove(CachePrefix + userId);
            }
        }

        public static void ClearAllCache()
        {
            var cache = HttpRuntime.Cache;
            if (cache == null)
            {
                return;
            }

            var keysToRemove = new List<string>();
            var enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key as string;
                if (key != null && key.StartsWith(CachePrefix, StringComparison.Ordinal))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                cache.Remove(key);
            }
        }
    }
}
