using PCShop.Models.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Areas.Admin.Controllers
{
    [RequirePermission("Product.Manage", "News.Manage", "Setting.Manage")]
    public class UploadController : Controller
    {
        private const int MaxFileSizeBytes = 10 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".jfif", ".png", ".gif", ".webp", ".ico" };

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        public ActionResult Image()
        {
            if (Request.Files == null || Request.Files.Count == 0)
            {
                return Json(new { success = false, message = "Không tìm thấy tệp ảnh tải lên." });
            }

            var uploadedUrls = new List<string>();
            var errors = new List<string>();

            for (var index = 0; index < Request.Files.Count; index++)
            {
                var file = Request.Files[index];
                string savedUrl;
                string errorMessage;
                if (TrySaveImage(file, out savedUrl, out errorMessage))
                {
                    uploadedUrls.Add(savedUrl);
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    errors.Add(errorMessage);
                }
            }

            if (uploadedUrls.Count == 0)
            {
                return Json(new
                {
                    success = false,
                    message = errors.FirstOrDefault() ?? "Không thể tải ảnh lên."
                });
            }

            return Json(new
            {
                success = true,
                urls = uploadedUrls,
                warnings = errors
            });
        }

        [HttpPost]
        public ActionResult CkEditorImage(HttpPostedFileBase upload, string CKEditorFuncNum, string responseType)
        {
            var isJson = string.Equals(responseType, "json", StringComparison.OrdinalIgnoreCase)
                || (Request.IsAjaxRequest() && string.IsNullOrWhiteSpace(CKEditorFuncNum));

            int numericFuncNum;
            var funcNumExpr = int.TryParse(CKEditorFuncNum, out numericFuncNum)
                ? numericFuncNum.ToString()
                : "'" + HttpUtility.JavaScriptStringEncode(CKEditorFuncNum ?? "") + "'";

            string savedUrl;
            string errorMessage;
            if (!TrySaveImage(upload, out savedUrl, out errorMessage))
            {
                if (isJson)
                {
                    return Json(new { uploaded = 0, error = new { message = errorMessage } });
                }

                var safeError = HttpUtility.JavaScriptStringEncode(errorMessage);
                return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + funcNumExpr + ", '', '" + safeError + "');</script>", "text/html");
            }

            if (isJson)
            {
                return Json(new
                {
                    uploaded = 1,
                    fileName = Path.GetFileName(savedUrl),
                    url = savedUrl
                });
            }

            var encodedUrl = HttpUtility.JavaScriptStringEncode(savedUrl);
            return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + funcNumExpr + ", '" + encodedUrl + "', '');</script>", "text/html");
        }

        [HttpGet]
        public ActionResult Browse(string CKEditorFuncNum)
        {
            ViewBag.CKEditorFuncNum = CKEditorFuncNum ?? "";
            var images = new List<string>();
            var rootPath = Server.MapPath("~/Uploads");

            if (Directory.Exists(rootPath))
            {
                var files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
                    .Where(path => path.IndexOf("_thumbs", StringComparison.OrdinalIgnoreCase) < 0)
                    .Where(path => AllowedExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
                    .OrderByDescending(path => System.IO.File.GetLastWriteTimeUtc(path))
                    .Take(120)
                    .ToList();

                var appRoot = Server.MapPath("~");
                foreach (var filePath in files)
                {
                    var relative = filePath.Substring(appRoot.Length).Replace("\\", "/");
                    if (!relative.StartsWith("/"))
                    {
                        relative = "/" + relative;
                    }
                    images.Add(relative);
                }
            }

            return View(images);
        }

        private bool TrySaveImage(HttpPostedFileBase file, out string savedUrl, out string errorMessage)
        {
            savedUrl = null;
            errorMessage = null;

            if (file == null || file.ContentLength <= 0)
            {
                errorMessage = "Tệp ảnh trống.";
                return false;
            }

            if (file.ContentLength > MaxFileSizeBytes)
            {
                errorMessage = "Ảnh vượt quá dung lượng tối đa 10 MB.";
                return false;
            }

            var extension = (Path.GetExtension(file.FileName) ?? "").ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Chỉ hỗ trợ định dạng JPG, JPEG, JFIF, PNG, GIF hoặc WEBP.";
                return false;
            }

            if (!HasValidImageSignature(file.InputStream))
            {
                errorMessage = "Tệp tải lên không phải là hình ảnh hợp lệ.";
                return false;
            }

            try
            {
                var folderSegment = DateTime.UtcNow.ToString("yyyyMM");
                var virtualFolder = "~/Uploads/images/" + folderSegment;
                var physicalFolder = Server.MapPath(virtualFolder);

                if (!Directory.Exists(physicalFolder))
                {
                    Directory.CreateDirectory(physicalFolder);
                }

                var saveExt = extension == ".jfif" ? ".jpg" : extension;
                var fileName = Guid.NewGuid().ToString("N") + saveExt;
                var physicalPath = Path.Combine(physicalFolder, fileName);
                file.SaveAs(physicalPath);

                savedUrl = "/Uploads/images/" + folderSegment + "/" + fileName;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Không thể lưu ảnh lên máy chủ: " + ex.Message;
                return false;
            }
        }

        private static bool HasValidImageSignature(Stream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                return false;
            }

            var originalPosition = stream.CanSeek ? stream.Position : 0;
            var header = new byte[12];
            var bytesRead = stream.Read(header, 0, header.Length);
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }

            if (bytesRead < 4)
            {
                return false;
            }

            var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            var isGif = header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38;
            var isIco = header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00;
            var isWebp = bytesRead >= 12
                && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            return isJpeg || isPng || isGif || isIco || isWebp;
        }
    }
}
