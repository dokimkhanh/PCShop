using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace PCShop.Models.Common
{
    public class SettingsHelper
    {
        private const int CacheMinutes = 10;
        private const string AllSettingsCacheKey = "SettingsHelper_AllValues";
        private static readonly object CacheLock = new object();

        public static string GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var map = GetAllSettings();
            string value;
            return map.TryGetValue(key, out value) && value != null ? value : string.Empty;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            var raw = GetValue(key);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            raw = raw.Trim();
            if (raw == "1" || string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (raw == "0" || string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return defaultValue;
        }

        private static Dictionary<string, string> GetAllSettings()
        {
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                var cached = cache.Get(AllSettingsCacheKey) as Dictionary<string, string>;
                if (cached != null)
                {
                    return cached;
                }
            }

            lock (CacheLock)
            {
                if (cache != null)
                {
                    var cached = cache.Get(AllSettingsCacheKey) as Dictionary<string, string>;
                    if (cached != null)
                    {
                        return cached;
                    }
                }

                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var rows = db.systemSettings
                            .AsNoTracking()
                            .Select(x => new { x.settingKey, x.settingValue })
                            .ToList();

                        foreach (var row in rows)
                        {
                            if (!string.IsNullOrWhiteSpace(row.settingKey))
                            {
                                result[row.settingKey] = row.settingValue ?? string.Empty;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return result;
                }

                if (cache != null)
                {
                    cache.Insert(AllSettingsCacheKey, result, null, DateTime.Now.AddMinutes(CacheMinutes), Cache.NoSlidingExpiration);
                }

                return result;
            }
        }

        public static void ClearCache(string key)
        {
            ClearAllCache();
        }

        public static void ClearAllCache()
        {
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                cache.Remove(AllSettingsCacheKey);
            }
        }
    }
}
