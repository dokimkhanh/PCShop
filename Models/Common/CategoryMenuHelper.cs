using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace PCShop.Models.Common
{
    public class CategoryMenuProduct
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Image { get; set; }
        public int EffectivePrice { get; set; }
    }

    public class CategoryMenuNode
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public int DirectProductCount { get; set; }
        public int TotalProductCount { get; set; }
        public List<CategoryMenuNode> Children { get; set; }
        public List<CategoryMenuProduct> FeaturedProducts { get; set; }

        public CategoryMenuNode()
        {
            Children = new List<CategoryMenuNode>();
            FeaturedProducts = new List<CategoryMenuProduct>();
        }
    }

    public static class CategoryMenuHelper
    {
        private const int CacheMinutes = 10;
        private const string TreeCacheKey = "CategoryMenuHelper_Tree";
        private static readonly object CacheLock = new object();

        private static readonly string[] PreferredRootOrder =
        {
            "pc-gaming",
            "pc-workstation-2d-3d",
            "pc-van-phong",
            "pc-mini-dong-bo",
            "laptop",
            "linh-kien-may-tinh",
            "man-hinh-may-tinh",
            "gaming-gear"
        };

        public static List<CategoryMenuNode> GetMenuTree()
        {
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                var cached = cache.Get(TreeCacheKey) as List<CategoryMenuNode>;
                if (cached != null)
                {
                    return cached;
                }
            }

            lock (CacheLock)
            {
                if (cache != null)
                {
                    var cached = cache.Get(TreeCacheKey) as List<CategoryMenuNode>;
                    if (cached != null)
                    {
                        return cached;
                    }
                }

                var tree = BuildTreeFromDb();
                if (cache != null)
                {
                    cache.Insert(TreeCacheKey, tree, null, DateTime.Now.AddMinutes(CacheMinutes), Cache.NoSlidingExpiration);
                }
                return tree;
            }
        }

        public static List<int> GetCategoryAndChildIds(int categoryId)
        {
            var ids = new List<int> { categoryId };
            var tree = GetMenuTree();
            var parent = tree.FirstOrDefault(x => x.Id == categoryId);
            if (parent != null && parent.Children.Count > 0)
            {
                foreach (var child in parent.Children)
                {
                    ids.Add(child.Id);
                }
            }
            return ids;
        }

        public static Dictionary<int, string> GetRootAliasByCategoryId()
        {
            var map = new Dictionary<int, string>();
            var tree = GetMenuTree();
            foreach (var root in tree)
            {
                map[root.Id] = root.Alias ?? string.Empty;
                foreach (var child in root.Children)
                {
                    map[child.Id] = root.Alias ?? string.Empty;
                }
            }
            return map;
        }

        public static CategoryMenuNode FindParentOf(int categoryId)
        {
            var tree = GetMenuTree();
            foreach (var root in tree)
            {
                if (root.Children.Any(c => c.Id == categoryId))
                {
                    return root;
                }
            }
            return null;
        }

        public static SelectList GetHierarchicalSelectList(int? selectedId = null)
        {
            var tree = GetMenuTree();
            var items = new List<SelectListItem>();
            foreach (var root in tree)
            {
                items.Add(new SelectListItem
                {
                    Value = root.Id.ToString(),
                    Text = root.Title,
                    Selected = selectedId.HasValue && selectedId.Value == root.Id
                });
                foreach (var child in root.Children)
                {
                    items.Add(new SelectListItem
                    {
                        Value = child.Id.ToString(),
                        Text = root.Title + " — " + child.Title,
                        Selected = selectedId.HasValue && selectedId.Value == child.Id
                    });
                }
            }
            return new SelectList(items, "Value", "Text", selectedId.HasValue ? selectedId.Value.ToString() : null);
        }

        public static void ClearCache()
        {
            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                cache.Remove(TreeCacheKey);
            }
        }

        private static List<CategoryMenuNode> BuildTreeFromDb()
        {
            var result = new List<CategoryMenuNode>();
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var categories = db.categories
                        .AsNoTracking()
                        .OrderBy(x => x.id)
                        .ToList();

                    var products = db.products
                        .Include(x => x.productImages)
                        .AsNoTracking()
                        .OrderByDescending(x => x.id)
                        .ToList();

                    var productsByCat = products
                        .GroupBy(x => x.categoryID)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var nodeMap = new Dictionary<int, CategoryMenuNode>();
                    foreach (var cat in categories)
                    {
                        List<Product> catProds;
                        var directCount = productsByCat.TryGetValue(cat.id, out catProds) ? catProds.Count : 0;
                        nodeMap[cat.id] = new CategoryMenuNode
                        {
                            Id = cat.id,
                            ParentId = cat.parentID,
                            Title = cat.title ?? string.Empty,
                            Alias = cat.alias ?? string.Empty,
                            Description = cat.description ?? string.Empty,
                            DirectProductCount = directCount,
                            TotalProductCount = directCount
                        };
                    }

                    foreach (var node in nodeMap.Values)
                    {
                        if (node.ParentId.HasValue && node.ParentId.Value != node.Id && nodeMap.ContainsKey(node.ParentId.Value))
                        {
                            nodeMap[node.ParentId.Value].Children.Add(node);
                        }
                        else
                        {
                            result.Add(node);
                        }
                    }

                    foreach (var root in result)
                    {
                        var allIds = new HashSet<int> { root.Id };
                        foreach (var ch in root.Children)
                        {
                            allIds.Add(ch.Id);
                        }

                        var rootProds = products.Where(p => allIds.Contains(p.categoryID)).ToList();
                        root.TotalProductCount = rootProds.Count;
                        root.FeaturedProducts = rootProds.Take(4).Select(p =>
                        {
                            var img = p.productImages != null
                                ? (p.productImages.FirstOrDefault(x => x.isDefault) ?? p.productImages.FirstOrDefault())
                                : null;
                            var effectivePrice = (p.priceSale > 0 && p.priceSale < p.price) ? p.priceSale : p.price;
                            return new CategoryMenuProduct
                            {
                                Id = p.id,
                                Title = p.title ?? string.Empty,
                                Alias = p.alias ?? string.Empty,
                                Image = img != null && !string.IsNullOrWhiteSpace(img.image) ? img.image : (p.image ?? string.Empty),
                                EffectivePrice = effectivePrice
                            };
                        }).ToList();
                    }

                    result = result
                        .OrderBy(r =>
                        {
                            var idx = Array.FindIndex(PreferredRootOrder, a => string.Equals(a, r.Alias, StringComparison.OrdinalIgnoreCase));
                            return idx >= 0 ? idx : 999;
                        })
                        .ThenBy(r => r.Id)
                        .ToList();
                }
            }
            catch (Exception)
            {
                return result;
            }
            return result;
        }
    }
}
