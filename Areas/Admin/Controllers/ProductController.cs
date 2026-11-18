using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        [RequirePermission("Product.View", "Product.Manage")]
        public ActionResult Index(string txtSearch, int? page)
        {
            var pageSize = 10;
            var pageIndex = page ?? 1;

            IQueryable<Product> query = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .AsNoTracking()
                .OrderByDescending(x => x.id);

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                query = query.Where(x => x.title.ToLower().Contains(keyword));
            }

            var productList = query.ToPagedList(pageIndex, pageSize);
            ViewBag._pageSize = pageSize;
            ViewBag._page = pageIndex;

            return View(productList);
        }

        [RequirePermission("Product.Manage")]
        public ActionResult Add()
        {
            ViewBag.productCategoryID = CategoryMenuHelper.GetHierarchicalSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Product.Manage")]
        public ActionResult Add(Product product, List<string> listImage, int? rdDefault)
        {
            if (ModelState.IsValid)
            {
                var cleanImages = (listImage ?? new List<string>())
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .Distinct()
                    .ToList();

                if (cleanImages.Count > 0)
                {
                    var defaultIdx = rdDefault.HasValue && rdDefault.Value >= 0 && rdDefault.Value < cleanImages.Count
                        ? rdDefault.Value
                        : 0;
                    for (var i = 0; i < cleanImages.Count; i++)
                    {
                        product.productImages.Add(new ProductImage
                        {
                            image = cleanImages[i],
                            isDefault = (i == defaultIdx)
                        });
                    }
                }

                product.productCode = string.IsNullOrWhiteSpace(product.productCode)
                    ? GenerateUniqueSku(product.categoryID)
                    : product.productCode.Trim().ToUpperInvariant();
                product.detail = HtmlSecurityHelper.Sanitize(product.detail);
                product.description = HtmlSecurityHelper.Sanitize(product.description);
                product.alias = CommonHelper.FilterChar(product.title);
                product.createdDate = DateTime.Now;
                product.createdBy = User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name)
                    ? User.Identity.Name
                    : "system";
                product.modifiedDate = DateTime.Now;

                dbContext.products.Add(product);
                dbContext.SaveChanges();
                CategoryMenuHelper.ClearCache();

                return RedirectToAction("Index");
            }
            ViewBag.productCategoryID = CategoryMenuHelper.GetHierarchicalSelectList(product.categoryID);
            ViewBag.UploadedImagesJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                (listImage ?? new List<string>()).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList());
            ViewBag.SelectedDefault = rdDefault ?? 0;
            return View(product);
        }

        [RequirePermission("Product.Manage")]
        public ActionResult Edit(int id)
        {
            var item = dbContext.products
                .Include(x => x.productImages)
                .FirstOrDefault(x => x.id == id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.productCategoryID = CategoryMenuHelper.GetHierarchicalSelectList(item.categoryID);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Product.Manage")]
        public ActionResult Edit(Product product, List<string> listImage, int? rdDefault)
        {
            if (ModelState.IsValid)
            {
                var existing = dbContext.products
                    .Include(x => x.productImages)
                    .FirstOrDefault(x => x.id == product.id);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.title = product.title;
                existing.productCode = string.IsNullOrWhiteSpace(product.productCode)
                    ? (string.IsNullOrWhiteSpace(existing.productCode) ? GenerateUniqueSku(product.categoryID) : existing.productCode)
                    : product.productCode.Trim().ToUpperInvariant();
                existing.description = HtmlSecurityHelper.Sanitize(product.description);
                existing.detail = HtmlSecurityHelper.Sanitize(product.detail);
                existing.modifiedDate = DateTime.Now;
                existing.modifiedBy = User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name)
                    ? User.Identity.Name
                    : "system";
                existing.alias = CommonHelper.FilterChar(product.title);
                existing.price = product.price;
                existing.priceSale = product.priceSale;
                existing.quantity = product.quantity;
                existing.isNew = product.isNew;
                existing.isSale = product.isSale;
                existing.categoryID = product.categoryID;

                var oldImages = existing.productImages.ToList();
                if (oldImages.Count > 0)
                {
                    dbContext.productImages.RemoveRange(oldImages);
                }

                var cleanImages = (listImage ?? new List<string>())
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url.Trim())
                    .Distinct()
                    .ToList();

                if (cleanImages.Count > 0)
                {
                    var defaultIdx = rdDefault.HasValue && rdDefault.Value >= 0 && rdDefault.Value < cleanImages.Count
                        ? rdDefault.Value
                        : 0;
                    for (var i = 0; i < cleanImages.Count; i++)
                    {
                        dbContext.productImages.Add(new ProductImage
                        {
                            ProductId = existing.id,
                            image = cleanImages[i],
                            isDefault = (i == defaultIdx)
                        });
                    }
                }

                dbContext.SaveChanges();
                CategoryMenuHelper.ClearCache();
                return RedirectToAction("Index");
            }

            ViewBag.productCategoryID = CategoryMenuHelper.GetHierarchicalSelectList(product.categoryID);
            product.productImages = dbContext.productImages.Where(x => x.ProductId == product.id).ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult Delete(int id)
        {
            var item = dbContext.products.Find(id);
            if (item != null)
            {
                dbContext.productImages.RemoveRange(dbContext.productImages.Where(x => x.ProductId == id));
                dbContext.products.Remove(item);
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                foreach (var raw in ids.Split(','))
                {
                    int id;
                    if (!int.TryParse(raw, out id))
                    {
                        continue;
                    }

                    var item = dbContext.products.Find(id);
                    if (item != null)
                    {
                        dbContext.productImages.RemoveRange(dbContext.productImages.Where(x => x.ProductId == id));
                        dbContext.products.Remove(item);
                    }
                }
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        [RequirePermission("Product.Manage")]
        public ActionResult GenerateSku(int? categoryId)
        {
            var sku = GenerateUniqueSku(categoryId ?? 0);
            return Json(new { success = true, sku = sku }, JsonRequestBehavior.AllowGet);
        }

        private string GenerateUniqueSku(int categoryId)
        {
            var catPrefix = "SP";
            if (categoryId > 0)
            {
                var catTitle = dbContext.categories
                    .AsNoTracking()
                    .Where(x => x.id == categoryId)
                    .Select(x => x.title)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(catTitle))
                {
                    var normalized = new string((CommonHelper.FilterChar(catTitle) ?? string.Empty)
                        .Where(char.IsLetterOrDigit)
                        .ToArray())
                        .ToUpperInvariant();
                    if (normalized.Length >= 3)
                    {
                        catPrefix = normalized.Substring(0, 3);
                    }
                    else if (normalized.Length > 0)
                    {
                        catPrefix = normalized;
                    }
                }
            }

            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            using (var rng = RandomNumberGenerator.Create())
            {
                for (var attempt = 0; attempt < 20; attempt++)
                {
                    var buffer = new byte[6];
                    rng.GetBytes(buffer);
                    var chars = new char[6];
                    for (var i = 0; i < 6; i++)
                    {
                        chars[i] = alphabet[buffer[i] % alphabet.Length];
                    }
                    var candidate = "PCS-" + catPrefix + "-" + new string(chars);
                    if (!dbContext.products.AsNoTracking().Any(x => x.productCode == candidate))
                    {
                        return candidate;
                    }
                }
            }

            return "PCS-" + catPrefix + "-" + DateTime.UtcNow.ToString("yyMMddHHmmss");
        }
    }
}
