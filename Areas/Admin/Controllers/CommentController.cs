using Microsoft.AspNet.Identity;
using PagedList;
using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    public class CommentThreadViewModel
    {
        public ProductComment Comment { get; set; }
        public List<ProductComment> Replies { get; set; }
        public bool HasAdminReply { get; set; }
    }

    public class CommentController : Controller
    {
        private ApplicationDbContext _dbContext = new ApplicationDbContext();

        [RequirePermission("Product.View", "Product.Manage")]
        public ActionResult Index(string txtSearch, int? productId, string status, int? page)
        {
            var pageSize = 10;
            var pageIndex = page ?? 1;

            var allComments = _dbContext.productComments
                .Include(c => c.Product)
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var repliesByParent = allComments
                .Where(c => c.ParentId.HasValue)
                .GroupBy(c => c.ParentId.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.CreatedDate).ToList());

            var threads = allComments
                .Where(c => !c.ParentId.HasValue)
                .Select(c =>
                {
                    List<ProductComment> replies;
                    if (!repliesByParent.TryGetValue(c.Id, out replies))
                    {
                        replies = new List<ProductComment>();
                    }
                    return new CommentThreadViewModel
                    {
                        Comment = c,
                        Replies = replies,
                        HasAdminReply = c.IsDeleted || c.IsAdminReply || replies.Any(r => r.IsAdminReply && !r.IsDeleted)
                    };
                })
                .Where(t => !t.Comment.IsDeleted || t.Replies.Count > 0)
                .ToList();

            ViewBag.TotalThreads = threads.Count;
            ViewBag.UnrepliedCount = threads.Count(t => !t.HasAdminReply);
            ViewBag.RepliedCount = threads.Count(t => t.HasAdminReply);

            IEnumerable<CommentThreadViewModel> filtered = threads;

            if (productId.HasValue && productId.Value > 0)
            {
                filtered = filtered.Where(t => t.Comment.ProductId == productId.Value);
            }

            if (string.Equals(status, "unreplied", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(t => !t.HasAdminReply);
            }
            else if (string.Equals(status, "replied", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(t => t.HasAdminReply);
            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                var kw = txtSearch.Trim().ToLower();
                filtered = filtered.Where(t =>
                    (t.Comment.AuthorName != null && t.Comment.AuthorName.ToLower().Contains(kw)) ||
                    (t.Comment.AuthorEmail != null && t.Comment.AuthorEmail.ToLower().Contains(kw)) ||
                    (t.Comment.Content != null && t.Comment.Content.ToLower().Contains(kw)) ||
                    (t.Comment.Product != null && t.Comment.Product.title != null && t.Comment.Product.title.ToLower().Contains(kw)) ||
                    t.Replies.Any(r =>
                        (r.AuthorName != null && r.AuthorName.ToLower().Contains(kw)) ||
                        (r.Content != null && r.Content.ToLower().Contains(kw))));
            }

            var paged = filtered.ToPagedList(pageIndex, pageSize);

            ViewBag.CurrentSearch = txtSearch;
            ViewBag.CurrentProductId = productId;
            ViewBag.CurrentStatus = status ?? "";
            ViewBag.ProductList = new SelectList(
                _dbContext.products.AsNoTracking().OrderBy(p => p.title).Select(p => new { p.id, p.title }).ToList(),
                "id",
                "title",
                productId);

            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult Reply(int parentId, string content)
        {
            var cleanContent = (content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanContent))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung phản hồi." });
            }
            if (cleanContent.Length > 2000)
            {
                return Json(new { success = false, message = "Nội dung tối đa 2.000 ký tự." });
            }

            var parent = _dbContext.productComments.Find(parentId);
            if (parent == null || parent.IsDeleted)
            {
                return Json(new { success = false, message = "Không thể phản hồi bình luận đã bị xoá." });
            }

            var rootId = parent.ParentId.HasValue ? parent.ParentId.Value : parent.Id;
            var userId = User.Identity.GetUserId();
            var currentUser = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
            var authorName = currentUser != null && !string.IsNullOrWhiteSpace(currentUser.FullName)
                ? currentUser.FullName
                : (!string.IsNullOrWhiteSpace(User.Identity.Name) ? User.Identity.Name : "Quản trị viên");

            var reply = new ProductComment
            {
                ProductId = parent.ProductId,
                ParentId = rootId,
                AuthorName = authorName,
                AuthorEmail = currentUser != null ? currentUser.Email : null,
                UserId = userId,
                Content = cleanContent,
                IsAdminReply = true,
                CreatedDate = DateTime.Now
            };

            _dbContext.productComments.Add(reply);
            _dbContext.SaveChanges();

            return Json(new
            {
                success = true,
                reply = new
                {
                    id = reply.Id,
                    parentId = rootId,
                    authorName = reply.AuthorName,
                    content = reply.Content,
                    createdDate = reply.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult Delete(int id)
        {
            var comment = _dbContext.productComments.Find(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận." });
            }

            if (!comment.ParentId.HasValue)
            {
                var hasReplies = _dbContext.productComments.Any(c => c.ParentId == id);
                if (hasReplies && !comment.IsDeleted)
                {
                    comment.IsDeleted = true;
                    comment.Content = "Bình luận đã bị xoá";
                    _dbContext.SaveChanges();
                    return Json(new { success = true, softDeleted = true, isReply = false });
                }

                var children = _dbContext.productComments.Where(c => c.ParentId == id).ToList();
                if (children.Count > 0)
                {
                    _dbContext.productComments.RemoveRange(children);
                }
                _dbContext.productComments.Remove(comment);
                _dbContext.SaveChanges();
                return Json(new { success = true, softDeleted = false, isReply = false });
            }

            if (!comment.IsDeleted)
            {
                comment.IsDeleted = true;
                comment.Content = "Bình luận đã bị xoá";
                _dbContext.SaveChanges();
                return Json(new { success = true, softDeleted = true, isReply = true });
            }

            var parentId = comment.ParentId.Value;
            _dbContext.productComments.Remove(comment);
            _dbContext.SaveChanges();

            int? removedParentId = null;
            var parent = _dbContext.productComments.Find(parentId);
            if (parent != null && parent.IsDeleted)
            {
                var remainingReplies = _dbContext.productComments.Any(c => c.ParentId == parentId);
                if (!remainingReplies)
                {
                    _dbContext.productComments.Remove(parent);
                    _dbContext.SaveChanges();
                    removedParentId = parentId;
                }
            }

            return Json(new { success = true, softDeleted = false, isReply = true, removedParentId = removedParentId });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        [RequirePermission("Product.Manage")]
        public ActionResult DeleteAll(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return Json(new { success = false });
            }

            foreach (var raw in ids.Split(','))
            {
                int id;
                if (!int.TryParse(raw, out id))
                {
                    continue;
                }
                var comment = _dbContext.productComments.Find(id);
                if (comment != null)
                {
                    var children = _dbContext.productComments.Where(c => c.ParentId == id).ToList();
                    if (children.Count > 0)
                    {
                        _dbContext.productComments.RemoveRange(children);
                    }
                    _dbContext.productComments.Remove(comment);
                }
            }

            _dbContext.SaveChanges();
            return Json(new { success = true });
        }
    }
}
