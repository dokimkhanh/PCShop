using Microsoft.AspNet.Identity;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        public ActionResult Index(int? id, string txtSearch, string sort)
        {
            var query = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .AsNoTracking()
                .AsQueryable();

            if (id != null)
            {
                query = query.Where(x => x.id == id);
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                query = query.Where(x =>
                    (x.title != null && x.title.ToLower().Contains(keyword)) ||
                    (x.description != null && x.description.ToLower().Contains(keyword)));
            }

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(x => x.priceSale > 0 ? x.priceSale : x.price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(x => x.priceSale > 0 ? x.priceSale : x.price);
                    break;
                case "name_asc":
                    query = query.OrderBy(x => x.title);
                    break;
                default:
                    query = query.OrderByDescending(x => x.id);
                    break;
            }

            ViewBag.CurrentSearch = txtSearch;
            ViewBag.CurrentSort = sort;
            return View(query.ToList());
        }

        public ActionResult ProductCategory(string alias, int? id, string txtSearch, string sort)
        {
            var query = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .AsNoTracking()
                .AsQueryable();

            if (id != null)
            {
                var catIds = CategoryMenuHelper.GetCategoryAndChildIds(id.Value);
                query = query.Where(x => catIds.Contains(x.categoryID));
                var currentCat = dbContext.categories.AsNoTracking().FirstOrDefault(x => x.id == id.Value);
                ViewBag.Category = currentCat;
                var parentNode = CategoryMenuHelper.FindParentOf(id.Value);
                if (parentNode != null)
                {
                    ViewBag.ParentCategory = parentNode;
                    ViewBag.SiblingCategories = parentNode.Children;
                }
                else
                {
                    var rootNode = CategoryMenuHelper.GetMenuTree().FirstOrDefault(x => x.Id == id.Value);
                    if (rootNode != null && rootNode.Children.Count > 0)
                    {
                        ViewBag.ChildCategories = rootNode.Children;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var keyword = txtSearch.Trim().ToLower();
                query = query.Where(x =>
                    (x.title != null && x.title.ToLower().Contains(keyword)) ||
                    (x.description != null && x.description.ToLower().Contains(keyword)));
            }

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(x => x.priceSale > 0 ? x.priceSale : x.price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(x => x.priceSale > 0 ? x.priceSale : x.price);
                    break;
                case "name_asc":
                    query = query.OrderBy(x => x.title);
                    break;
                default:
                    query = query.OrderByDescending(x => x.id);
                    break;
            }

            ViewBag.CategoryID = id;
            ViewBag.CurrentSearch = txtSearch;
            ViewBag.CurrentSort = sort;
            return View(query.ToList());
        }

        public ActionResult ItemByCategory()
        {
            var tree = CategoryMenuHelper.GetMenuTree();
            var rootMap = CategoryMenuHelper.GetRootAliasByCategoryId();
            var allRecent = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .AsNoTracking()
                .OrderByDescending(x => x.id)
                .ToList();

            var balanced = new System.Collections.Generic.List<Product>();
            foreach (var root in tree)
            {
                var groupItems = allRecent
                    .Where(p => rootMap.ContainsKey(p.categoryID) && rootMap[p.categoryID] == root.Alias)
                    .Take(4)
                    .ToList();
                balanced.AddRange(groupItems);
            }

            if (balanced.Count == 0)
            {
                balanced = allRecent.Take(16).ToList();
            }

            ViewBag.RootAliasByCategoryId = rootMap;
            return PartialView("_ItemByCategory", balanced);
        }

        public ActionResult ItemProductSale()
        {
            var items = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .AsNoTracking()
                .Where(x => x.isSale || (x.priceSale > 0 && x.priceSale < x.price))
                .OrderByDescending(x => x.id)
                .Take(8)
                .ToList();
            return PartialView("_ItemProductSale", items);
        }

        public ActionResult ProductDetail(string alias, int id)
        {
            var item = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .FirstOrDefault(x => x.id == id);
            if (item == null)
            {
                return HttpNotFound();
            }

            var comments = dbContext.productComments
                .AsNoTracking()
                .Where(c => c.ProductId == id)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var replyParents = new System.Collections.Generic.HashSet<int>(
                comments
                    .Where(c => c.ParentId.HasValue)
                    .Select(c => c.ParentId.Value));

            comments = comments
                .Where(c => c.ParentId.HasValue || !c.IsDeleted || replyParents.Contains(c.Id))
                .ToList();

            ViewBag.Comments = comments;
            ViewBag.CanModerateComments = User != null && User.Identity != null && User.Identity.IsAuthenticated
                && PermissionHelper.HasPermission(User, "Product.Manage");

            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var currentUser = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
                ViewBag.CurrentUserName = currentUser != null && !string.IsNullOrWhiteSpace(currentUser.FullName)
                    ? currentUser.FullName
                    : User.Identity.Name;
                ViewBag.CurrentUserEmail = currentUser != null ? currentUser.Email : "";
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        public ActionResult AddComment(int productId, string authorName, string authorEmail, string content, int? parentId)
        {
            var productExists = dbContext.products.Any(p => p.id == productId);
            if (!productExists)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            var cleanContent = (content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanContent))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung bình luận." });
            }
            if (cleanContent.Length > 2000)
            {
                return Json(new { success = false, message = "Nội dung bình luận tối đa 2.000 ký tự." });
            }

            int? resolvedParentId = null;
            if (parentId.HasValue)
            {
                var parentComment = dbContext.productComments.AsNoTracking()
                    .FirstOrDefault(c => c.Id == parentId.Value && c.ProductId == productId);
                if (parentComment == null)
                {
                    return Json(new { success = false, message = "Bình luận gốc không tồn tại." });
                }
                if (parentComment.IsDeleted)
                {
                    return Json(new { success = false, message = "Không thể trả lời bình luận đã bị xoá." });
                }
                var rootId = parentComment.ParentId.HasValue ? parentComment.ParentId.Value : parentComment.Id;
                var rootComment = rootId == parentComment.Id
                    ? parentComment
                    : dbContext.productComments.AsNoTracking().FirstOrDefault(c => c.Id == rootId);
                if (rootComment == null || rootComment.IsDeleted)
                {
                    return Json(new { success = false, message = "Không thể trả lời bình luận đã bị xoá." });
                }
                resolvedParentId = rootId;
            }

            var cleanName = (authorName ?? string.Empty).Trim();
            var cleanEmail = (authorEmail ?? string.Empty).Trim();
            string userId = null;
            var isAdminReply = false;

            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = User.Identity.GetUserId();
                isAdminReply = PermissionHelper.HasPermission(User, "Product.Manage");
                var currentUser = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
                if (string.IsNullOrWhiteSpace(cleanName))
                {
                    cleanName = currentUser != null && !string.IsNullOrWhiteSpace(currentUser.FullName)
                        ? currentUser.FullName
                        : User.Identity.Name;
                }
                if (string.IsNullOrWhiteSpace(cleanEmail) && currentUser != null)
                {
                    cleanEmail = currentUser.Email ?? string.Empty;
                }
            }

            if (string.IsNullOrWhiteSpace(cleanName))
            {
                return Json(new { success = false, message = "Vui lòng nhập tên hiển thị của bạn." });
            }
            if (cleanName.Length > 120)
            {
                cleanName = cleanName.Substring(0, 120);
            }
            if (cleanEmail.Length > 180)
            {
                cleanEmail = cleanEmail.Substring(0, 180);
            }

            var comment = new ProductComment
            {
                ProductId = productId,
                ParentId = resolvedParentId,
                AuthorName = cleanName,
                AuthorEmail = string.IsNullOrWhiteSpace(cleanEmail) ? null : cleanEmail,
                UserId = userId,
                Content = cleanContent,
                IsAdminReply = isAdminReply,
                CreatedDate = DateTime.Now
            };

            dbContext.productComments.Add(comment);
            dbContext.SaveChanges();

            var initial = cleanName.Substring(0, 1).ToUpperInvariant();
            return Json(new
            {
                success = true,
                comment = new
                {
                    id = comment.Id,
                    parentId = comment.ParentId,
                    authorName = comment.AuthorName,
                    content = comment.Content,
                    isAdminReply = comment.IsAdminReply,
                    createdDate = comment.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    initial = initial
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult DeleteComment(int id)
        {
            var comment = dbContext.productComments.Find(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận." });
            }

            if (!comment.ParentId.HasValue)
            {
                var hasReplies = dbContext.productComments.Any(c => c.ParentId == id);
                if (hasReplies && !comment.IsDeleted)
                {
                    comment.IsDeleted = true;
                    comment.Content = "Bình luận đã bị xoá";
                    dbContext.SaveChanges();
                    return Json(new
                    {
                        success = true,
                        softDeleted = true,
                        isReply = false,
                        removedCount = 1,
                        createdDate = comment.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                    });
                }

                var children = dbContext.productComments.Where(c => c.ParentId == id).ToList();
                var removedCount = (comment.IsDeleted ? 0 : 1) + children.Count(c => !c.IsDeleted);
                if (children.Count > 0)
                {
                    dbContext.productComments.RemoveRange(children);
                }
                dbContext.productComments.Remove(comment);
                dbContext.SaveChanges();
                return Json(new { success = true, softDeleted = false, isReply = false, removedCount = removedCount });
            }

            if (!comment.IsDeleted)
            {
                comment.IsDeleted = true;
                comment.Content = "Bình luận đã bị xoá";
                dbContext.SaveChanges();
                return Json(new
                {
                    success = true,
                    softDeleted = true,
                    isReply = true,
                    removedCount = 1,
                    createdDate = comment.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                });
            }

            var parentId = comment.ParentId.Value;
            dbContext.productComments.Remove(comment);
            dbContext.SaveChanges();

            int? removedParentId = null;
            var parent = dbContext.productComments.Find(parentId);
            if (parent != null && parent.IsDeleted)
            {
                var remainingReplies = dbContext.productComments.Any(c => c.ParentId == parentId);
                if (!remainingReplies)
                {
                    dbContext.productComments.Remove(parent);
                    dbContext.SaveChanges();
                    removedParentId = parentId;
                }
            }

            return Json(new
            {
                success = true,
                softDeleted = false,
                isReply = true,
                removedCount = 0,
                removedParentId = removedParentId
            });
        }
    }
}
