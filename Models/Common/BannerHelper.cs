using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace PCShop.Models.Common
{
    public class HomeBannerItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
    }

    public static class BannerHelper
    {
        private const int CacheMinutes = 10;
        private const string CacheKey = "BannerHelper_ActiveBanners";
        private static readonly object CacheLock = new object();

        public static List<HomeBannerItem> GetActiveBanners()
        {
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                var cached = cache.Get(CacheKey) as List<HomeBannerItem>;
                if (cached != null)
                {
                    return cached;
                }
            }

            lock (CacheLock)
            {
                if (cache != null)
                {
                    var cached = cache.Get(CacheKey) as List<HomeBannerItem>;
                    if (cached != null)
                    {
                        return cached;
                    }
                }

                var list = new List<HomeBannerItem>();
                try
                {
                    using (var db = new ApplicationDbContext())
                    {
                        list = db.advs
                            .AsNoTracking()
                            .Where(x => x.type == 1 && x.image != null && x.image != "")
                            .OrderBy(x => x.id)
                            .Select(x => new HomeBannerItem
                            {
                                Id = x.id,
                                Title = x.title,
                                Description = x.description,
                                Image = x.image,
                                Link = x.link
                            })
                            .ToList();
                    }
                }
                catch (Exception)
                {
                    list = new List<HomeBannerItem>();
                }

                if (list.Count == 0)
                {
                    var fallback = SettingsHelper.GetValue("SettingBanner");
                    if (string.IsNullOrWhiteSpace(fallback))
                    {
                        fallback = "/Content/assets/images/slider_1.jpg";
                    }
                    list.Add(new HomeBannerItem
                    {
                        Id = 0,
                        Title = "PCShop — PC Gaming & Linh Kiện Chính Hãng",
                        Image = fallback,
                        Link = "/san-pham"
                    });
                }

                if (cache != null)
                {
                    cache.Insert(CacheKey, list, null, DateTime.Now.AddMinutes(CacheMinutes), Cache.NoSlidingExpiration);
                }

                return list;
            }
        }

        public static void ClearCache()
        {
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                cache.Remove(CacheKey);
            }
        }
    }
}
