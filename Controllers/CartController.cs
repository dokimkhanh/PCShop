using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class CartController : Controller
    {
        private const int PaymentStatusPending = 0;
        private const int PaymentStatusCompleted = 1;

        private const int PaymentTypeCod = PaymentConfigHelper.PaymentTypeCod;
        private const string CartCookieName = "PCShop_Cart";

        private ApplicationDbContext dbContext = new ApplicationDbContext();

        private Cart BuildCartFromPairs(string rawPairs)
        {
            var cart = new Cart();
            if (string.IsNullOrWhiteSpace(rawPairs))
            {
                return cart;
            }

            var requested = new System.Collections.Generic.Dictionary<int, int>();
            foreach (var part in rawPairs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var segs = part.Split(':');
                int productId;
                int qty;
                if (segs.Length == 2 && int.TryParse(segs[0], out productId) && int.TryParse(segs[1], out qty) && productId > 0 && qty > 0)
                {
                    if (requested.ContainsKey(productId))
                    {
                        requested[productId] += qty;
                    }
                    else
                    {
                        requested[productId] = qty;
                    }
                }
            }

            if (requested.Count == 0)
            {
                return cart;
            }

            var ids = requested.Keys.ToList();
            var products = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .AsNoTracking()
                .Where(x => ids.Contains(x.id))
                .ToList();

            foreach (var product in products)
            {
                var qty = requested[product.id];
                var defaultImg = product.productImages != null
                    ? (product.productImages.FirstOrDefault(x => x.isDefault) ?? product.productImages.FirstOrDefault())
                    : null;
                var price = (product.priceSale > 0 && product.priceSale < product.price) ? product.priceSale : product.price;
                if (price < 0)
                {
                    price = 0;
                }

                cart.AddItem(new CartItems
                {
                    Id = product.id,
                    productName = product.title,
                    Alias = product.alias,
                    categoryName = product.productCategory != null ? product.productCategory.title : string.Empty,
                    productImg = defaultImg != null ? defaultImg.image : string.Empty,
                    price = price,
                    quantity = qty,
                    totalPrice = price * qty
                }, qty);
            }

            return cart;
        }

        private Cart GetCart()
        {
            var cart = Session["Cart"] as Cart;
            if (cart != null && cart.listCartItem != null && cart.listCartItem.Any())
            {
                return cart;
            }

            var cookie = Request.Cookies[CartCookieName];
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                var hydrated = BuildCartFromPairs(HttpUtility.UrlDecode(cookie.Value));
                if (hydrated.listCartItem.Any())
                {
                    Session["Cart"] = hydrated;
                    return hydrated;
                }
            }

            return cart;
        }

        private void SaveCart(Cart cart)
        {
            if (cart == null || cart.listCartItem == null || !cart.listCartItem.Any())
            {
                Session["Cart"] = null;
                var expired = new HttpCookie(CartCookieName, string.Empty)
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(-1)
                };
                Response.Cookies.Set(expired);
                return;
            }

            Session["Cart"] = cart;
            var serialized = string.Join(",", cart.listCartItem.Select(i => i.Id + ":" + i.quantity));
            var cookie = new HttpCookie(CartCookieName, HttpUtility.UrlEncode(serialized))
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Set(cookie);
        }

        public ActionResult Index()
        {
            Cart cart = GetCart();
            if (cart != null && cart.listCartItem.Any())
            {
                return View(cart.listCartItem);
            }
            return View();
        }

        public ActionResult CheckOut()
        {
            Cart cart = GetCart();
            if (cart != null && cart.listCartItem.Any())
            {
                ViewBag.PaymentMethods = PaymentConfigHelper.GetAllMethods().Where(x => x.IsEnabled).ToList();
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var currentName = User.Identity.Name;
                    var currentUser = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == currentName);
                    if (currentUser != null)
                    {
                        ViewBag.UserFullName = currentUser.FullName ?? string.Empty;
                        ViewBag.UserPhone = currentUser.Phone ?? string.Empty;
                        ViewBag.UserEmail = currentUser.Email ?? string.Empty;
                    }
                }
                return View(cart.listCartItem);
            }
            return RedirectToAction("Index", "Cart");
        }

        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = PaymentConfigHelper.GetVnpayConfig().HashSecret;
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                if (string.IsNullOrEmpty(vnp_HashSecret))
                {
                    Trace.TraceError("VNPAY: Thieu cau hinh vnp_HashSecret");
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý";
                    return View();
                }

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (!checkSignature)
                {
                    Trace.TraceError("VNPAY: Chu ky khong hop le, InputData=" + Request.RawUrl);
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý";
                    return View();
                }

                long vnp_AmountRaw;
                long vnpayTranId;
                if (!long.TryParse(vnpay.GetResponseData("vnp_Amount"), out vnp_AmountRaw) ||
                    !long.TryParse(vnpay.GetResponseData("vnp_TransactionNo"), out vnpayTranId))
                {
                    Trace.TraceError("VNPAY: Du lieu tra ve khong hop le (vnp_Amount/vnp_TransactionNo), InputData=" + Request.RawUrl);
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý";
                    return View();
                }

                string orderCode = vnpay.GetResponseData("vnp_TxnRef");
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                long vnp_Amount = vnp_AmountRaw / 100;

                ViewBag.Total = vnp_Amount;

                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    var order = dbContext.orders.FirstOrDefault(x => x.code == orderCode);
                    if (order == null)
                    {
                        Trace.TraceError("VNPAY: Khong tim thay don hang, OrderCode=" + orderCode);
                        ViewBag.InnerText = "Không tìm thấy đơn hàng " + orderCode + " trong hệ thống. Vui lòng liên hệ bộ phận hỗ trợ.";
                        return View();
                    }

                    if (vnp_Amount != order.totalAmount)
                    {
                        Trace.TraceError(string.Format("VNPAY: So tien thanh toan khong khop, OrderCode={0}, VNPAY={1}, Order={2}, TranId={3}",
                            orderCode, vnp_Amount, order.totalAmount, vnpayTranId));
                        ViewBag.InnerText = "Số tiền thanh toán không khớp với đơn hàng. Vui lòng liên hệ bộ phận hỗ trợ.";
                        return View();
                    }

                    if (order.paymentStatus != PaymentStatusCompleted)
                    {
                        order.paymentStatus = PaymentStatusCompleted;
                        order.modifiedDate = DateTime.Now;
                        order.modifiedBy = "VNPAY";
                        dbContext.SaveChanges();

                        SendOrderConfirmationMail(order, null);

                        Cart cart = GetCart();
                        if (cart != null)
                        {
                            cart.ClearCart();
                        }
                        SaveCart(null);
                    }

                    ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ.";
                }
                else
                {
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý" + vnp_ResponseCode;
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(CheckoutViewModel model, string[] items)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                return Json(new { Success = false, Code = -1, Url = "", msg = error ?? "Thông tin đặt hàng không hợp lệ." });
            }

            if (!IsPaymentMethodEnabled(model.paymentType))
            {
                return Json(new { Success = false, Code = -1, Url = "", msg = "Phương thức thanh toán này hiện đang tạm tắt hoặc không hợp lệ." });
            }

            Cart cart = GetCart();
            if ((cart == null || cart.listCartItem == null || !cart.listCartItem.Any()) && items != null && items.Length > 0)
            {
                cart = BuildCartFromPairs(string.Join(",", items));
            }
            if (cart == null || cart.listCartItem == null || !cart.listCartItem.Any())
            {
                return Json(new { Success = false, Code = -1, Url = "", msg = "Giỏ hàng của bạn đang trống." });
            }

            var order = new Order
            {
                code = "PCS" + CommonHelper.RandomNumber(9),
                customerName = (model.customerName ?? string.Empty).Trim(),
                phone = (model.phone ?? string.Empty).Trim(),
                address = (model.address ?? string.Empty).Trim(),
                email = (model.email ?? string.Empty).Trim(),
                paymentType = model.paymentType,
                paymentStatus = PaymentStatusPending,
                createdBy = User.Identity.IsAuthenticated ? User.Identity.Name : "Guest",
                createdDate = DateTime.Now,
                modifiedDate = DateTime.Now
            };

            long totalAmount = 0;
            int totalQuantity = 0;

            foreach (var item in cart.listCartItem)
            {
                if (item == null || item.Id <= 0 || item.quantity < 1)
                {
                    return Json(new { Success = false, Code = -1, Url = "", msg = "Giỏ hàng có sản phẩm không hợp lệ. Vui lòng làm mới giỏ hàng và thử lại." });
                }

                var product = dbContext.products.FirstOrDefault(x => x.id == item.Id);
                if (product == null)
                {
                    return Json(new { Success = false, Code = -1, Url = "", msg = "Sản phẩm trong giỏ hàng không còn tồn tại. Vui lòng làm mới giỏ hàng và thử lại." });
                }

                if (item.quantity > product.quantity)
                {
                    string msg = product.quantity <= 0
                        ? string.Format("Sản phẩm {0} đã hết hàng. Vui lòng xoá khỏi giỏ hàng.", product.title)
                        : string.Format("Sản phẩm {0} chỉ còn {1} sản phẩm. Vui lòng cập nhật lại số lượng.", product.title, product.quantity);

                    return Json(new { Success = false, Code = -1, Url = "", msg = msg });
                }

                int price = product.priceSale > 0 ? product.priceSale : product.price;
                if (price < 0)
                {
                    price = 0;
                }

                totalAmount += (long)price * item.quantity;
                if (totalAmount > int.MaxValue)
                {
                    return Json(new { Success = false, Code = -1, Url = "", msg = "Tổng giá trị đơn hàng quá lớn. Vui lòng liên hệ bộ phận hỗ trợ." });
                }

                totalQuantity += item.quantity;

                item.price = price;
                item.totalPrice = price * item.quantity;

                order.orderDetails.Add(new OrderDetail
                {
                    productID = product.id,
                    quantity = item.quantity,
                    price = price
                });
            }

            order.totalAmount = (int)totalAmount;
            order.quantity = totalQuantity;

            dbContext.orders.Add(order);
            dbContext.SaveChanges();

            SaveCart(cart);

            if (order.paymentType == PaymentTypeCod)
            {
                SendOrderConfirmationMail(order, cart);

                TempData["LastOrderCode"] = order.code;
                cart.ClearCart();
                SaveCart(null);

                return Json(new { Success = true, Code = 2, Url = "" });
            }

            var url = UrlPayment(order.paymentType, order.code);
            if (string.IsNullOrWhiteSpace(url))
            {
                dbContext.orders.Remove(order);
                dbContext.SaveChanges();
                Trace.TraceError("VNPAY: Khong tao duoc URL thanh toan cho don hang " + order.code);
                return Json(new { Success = false, Code = -1, Url = "", msg = "Không tạo được liên kết thanh toán. Vui lòng thử lại hoặc chọn thanh toán khi nhận hàng." });
            }

            return Json(new { Success = true, Code = 1, Url = url });
        }

        public ActionResult CheckOutSuccess()
        {
            var lastCode = TempData["LastOrderCode"] as string;
            Order order = null;
            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                order = dbContext.orders.AsNoTracking().FirstOrDefault(x => x.code == lastCode);
            }
            return View(order);
        }

        public ActionResult ShowCount()
        {
            Cart cart = GetCart();
            if (cart != null)
            {
                return Json(new { Count = cart.GetTotalQuantity() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        public ActionResult AddItem(int id, int quantity)
        {
            if (quantity < 1)
            {
                return Json(new { Success = false, msg = "Số lượng sản phẩm không hợp lệ", Code = -1 });
            }

            var checkProduct = dbContext.products
                .Include(x => x.productCategory)
                .Include(x => x.productImages)
                .FirstOrDefault(x => x.id == id);
            if (checkProduct == null)
            {
                return Json(new { Success = false, msg = "Sản phẩm không tồn tại", Code = -1 });
            }

            Cart cart = GetCart();
            if (cart == null)
            {
                cart = new Cart();
            }

            var checkExits = cart.listCartItem.FirstOrDefault(x => x.Id == id);
            int currentQuantity = checkExits != null ? checkExits.quantity : 0;
            int totalRequested = currentQuantity + quantity;

            if (totalRequested > checkProduct.quantity)
            {
                string msg = checkProduct.quantity <= 0
                    ? "Sản phẩm đã hết hàng"
                    : string.Format("Sản phẩm chỉ còn {0} sản phẩm trong kho", checkProduct.quantity);
                return Json(new { Success = false, msg = msg, Code = -1 });
            }

            var defaultImg = checkProduct.productImages != null
                ? (checkProduct.productImages.FirstOrDefault(x => x.isDefault) ?? checkProduct.productImages.FirstOrDefault())
                : null;
            var effectivePrice = (checkProduct.priceSale > 0 && checkProduct.priceSale < checkProduct.price)
                ? checkProduct.priceSale
                : checkProduct.price;

            CartItems cartItems = new CartItems
            {
                Id = checkProduct.id,
                productName = checkProduct.title,
                Alias = checkProduct.alias,
                categoryName = checkProduct.productCategory != null ? checkProduct.productCategory.title : string.Empty,
                productImg = defaultImg != null ? defaultImg.image : string.Empty,
                price = effectivePrice,
                quantity = quantity,
                totalPrice = effectivePrice * quantity
            };

            cart.AddItem(cartItems, quantity);
            SaveCart(cart);

            return Json(new { Success = true, msg = "Thêm sản phẩm thành công", Code = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        public ActionResult DeleteItem(int id)
        {
            var code = new { Success = false, msg = "Không thể xoá sản phẩm", Code = -1 };

            Cart cart = GetCart();
            if (cart != null)
            {
                var checkCart = cart.listCartItem.FirstOrDefault(x => x.Id == id);
                if (checkCart != null)
                {
                    cart.DeleteItem(id);
                    SaveCart(cart);
                    code = new { Success = true, msg = "Đã xoá sản phẩm khỏi giỏ hàng", Code = 1 };
                }
            }
            return Json(code);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        public ActionResult ClearCart()
        {
            Cart cart = GetCart();
            if (cart != null)
            {
                cart.ClearCart();
            }
            SaveCart(null);
            return Json(new { Success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAjax]
        public ActionResult UpdateQuantityCartItem(int id, int quantity)
        {
            if (quantity < 1)
            {
                return Json(new { Success = false, msg = "Số lượng sản phẩm không hợp lệ" });
            }

            Cart cart = GetCart();
            if (cart == null)
            {
                return Json(new { Success = false, msg = "Giỏ hàng của bạn đang trống" });
            }

            var checkCart = cart.listCartItem.FirstOrDefault(x => x.Id == id);
            if (checkCart == null)
            {
                return Json(new { Success = false, msg = "Sản phẩm không có trong giỏ hàng" });
            }

            var product = dbContext.products.FirstOrDefault(x => x.id == id);
            if (product == null)
            {
                return Json(new { Success = false, msg = "Sản phẩm không tồn tại" });
            }

            if (quantity > product.quantity)
            {
                string msg = product.quantity <= 0
                    ? "Sản phẩm đã hết hàng"
                    : string.Format("Sản phẩm chỉ còn {0} sản phẩm trong kho", product.quantity);
                return Json(new { Success = false, msg = msg });
            }

            cart.UpdateQuantity(id, quantity);
            SaveCart(cart);

            return Json(new { Success = true });
        }

        public string UrlPayment(int TypePaymentVN, string OrderCode)
        {
            string paymentUrl = "";
            var order = dbContext.orders.FirstOrDefault(x => x.code == OrderCode);
            if (order == null)
            {
                return "";
            }

            var vnpayConfig = PaymentConfigHelper.GetVnpayConfig();
            string vnp_Returnurl = vnpayConfig.ReturnUrl;
            string vnp_Url = vnpayConfig.Url;
            string vnp_TmnCode = vnpayConfig.TmnCode;
            string vnp_HashSecret = vnpayConfig.HashSecret;

            if (string.IsNullOrWhiteSpace(vnp_Url) ||
                string.IsNullOrWhiteSpace(vnp_TmnCode) ||
                string.IsNullOrWhiteSpace(vnp_HashSecret) ||
                string.IsNullOrWhiteSpace(vnp_Returnurl))
            {
                Trace.TraceError("VNPAY: Thieu cau hinh (vnp_Url/vnp_TmnCode/vnp_HashSecret/vnp_Returnurl)");
                return "";
            }

            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)order.totalAmount * 100).ToString());
            if (TypePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (TypePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (TypePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            vnpay.AddRequestData("vnp_CreateDate", order.createdDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + order.code);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.code);


            paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;

        }

        private static bool IsPaymentMethodEnabled(int paymentType)
        {
            return PaymentConfigHelper.IsMethodEnabled(paymentType);
        }

        private void SendOrderConfirmationMail(Order order, Cart cart)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.email))
            {
                return;
            }

            try
            {
                var shopName = SettingsHelper.GetValue("SettingTitle");
                if (string.IsNullOrWhiteSpace(shopName))
                {
                    shopName = "PCShop";
                }
                string contentCustomer = BuildOrderEmailContent(order, cart);
                MailHelper.SendMail(shopName, "Xác nhận đơn hàng #" + order.code, contentCustomer, order.email);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Không gửi được email xác nhận đơn hàng " + order.code + ": " + ex);
            }
        }

        private string BuildOrderEmailContent(Order order, Cart cart)
        {
            var strSanPham = new StringBuilder();
            long tongtien = 0;
            int tongSoLuong = 0;
            string baseUrl = Request != null && Request.Url != null
                ? Request.Url.GetLeftPart(UriPartial.Authority)
                : string.Empty;

            if (cart != null && cart.listCartItem != null && cart.listCartItem.Any())
            {
                foreach (var item in cart.listCartItem)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    long thanhTien = (long)item.price * item.quantity;
                    tongtien += thanhTien;
                    tongSoLuong += item.quantity;
                    strSanPham.Append(BuildOrderEmailRow(baseUrl, item.productImg, item.productName, item.price, item.quantity, thanhTien));
                }
            }
            else if (order != null && order.orderDetails != null && order.orderDetails.Any())
            {
                var productIds = order.orderDetails.Select(x => x.productID).Distinct().ToList();
                var products = dbContext.products
                    .AsNoTracking()
                    .Include(x => x.productImages)
                    .Where(x => productIds.Contains(x.id))
                    .ToList()
                    .ToDictionary(x => x.id);

                foreach (var detail in order.orderDetails)
                {
                    Product product;
                    products.TryGetValue(detail.productID, out product);

                    string productName = product != null ? product.title : ("Sản phẩm #" + detail.productID);
                    string productImg = string.Empty;
                    if (product != null)
                    {
                        var defaultImage = product.productImages.FirstOrDefault(x => x.isDefault);
                        productImg = defaultImage != null ? defaultImage.image : product.image;
                    }

                    long thanhTien = (long)detail.price * detail.quantity;
                    tongtien += thanhTien;
                    tongSoLuong += detail.quantity;
                    strSanPham.Append(BuildOrderEmailRow(baseUrl, productImg, productName, detail.price, detail.quantity, thanhTien));
                }
            }

            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/mail1.html"), Encoding.UTF8);
            string orderCode = order != null ? (order.code ?? string.Empty) : string.Empty;
            string customerName = order != null ? (order.customerName ?? string.Empty) : string.Empty;
            string customerPhone = order != null ? (order.phone ?? string.Empty) : string.Empty;
            string customerAddress = order != null ? (order.address ?? string.Empty) : string.Empty;
            int paymentType = order != null ? order.paymentType : PaymentTypeCod;
            bool isPaid = order != null && order.paymentStatus == PaymentStatusCompleted;

            string shopName = SettingsHelper.GetValue("SettingTitle");
            if (string.IsNullOrWhiteSpace(shopName))
            {
                shopName = "PCShop";
            }
            string shopAddress = (SettingsHelper.GetValue("SettingAddress") ?? string.Empty).Trim();
            string shopHotline = (SettingsHelper.GetValue("SettingHotline") ?? string.Empty).Trim();
            string shopEmail = (SettingsHelper.GetValue("SettingEmail") ?? string.Empty).Trim();

            string shopAddressLine = string.IsNullOrWhiteSpace(shopAddress)
                ? string.Empty
                : " &middot; " + HttpUtility.HtmlEncode(shopAddress);

            var contactParts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(shopHotline))
            {
                contactParts.Add("Hotline: <strong style=\"color:#3f3f46;\">" + HttpUtility.HtmlEncode(shopHotline) + "</strong>");
            }
            if (!string.IsNullOrWhiteSpace(shopEmail))
            {
                var safeEmail = HttpUtility.HtmlEncode(shopEmail);
                contactParts.Add("<a href=\"mailto:" + HttpUtility.HtmlAttributeEncode(shopEmail) + "\" style=\"color:#52525b;text-decoration:underline;\">" + safeEmail + "</a>");
            }
            string shopContactLine = contactParts.Count > 0
                ? "<div style=\"margin-top:4px;\">Cần hỗ trợ? " + string.Join(" &middot; ", contactParts) + "</div>"
                : string.Empty;

            string statusText = isPaid
                ? "Đã thanh toán thành công"
                : (paymentType == PaymentTypeCod ? "Thanh toán khi nhận hàng (COD)" : "Đang chờ thanh toán");
            string statusBg = isPaid ? "#ecfdf5" : "#f4f4f5";
            string statusColor = isPaid ? "#047857" : "#3f3f46";

            contentCustomer = contentCustomer.Replace("{{ShopName}}", HttpUtility.HtmlEncode(shopName));
            contentCustomer = contentCustomer.Replace("{{ShopAddressLine}}", shopAddressLine);
            contentCustomer = contentCustomer.Replace("{{ShopContactLine}}", shopContactLine);
            contentCustomer = contentCustomer.Replace("{{Year}}", DateTime.Now.Year.ToString());
            contentCustomer = contentCustomer.Replace("{{TrangThaiThanhToan}}", HttpUtility.HtmlEncode(statusText));
            contentCustomer = contentCustomer.Replace("{{TrangThaiThanhToanBg}}", statusBg);
            contentCustomer = contentCustomer.Replace("{{TrangThaiThanhToanColor}}", statusColor);
            contentCustomer = contentCustomer.Replace("{{MaDonHang}}", HttpUtility.HtmlEncode(orderCode));
            contentCustomer = contentCustomer.Replace("{{TongSoLuong}}", tongSoLuong.ToString());
            contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham.ToString());
            contentCustomer = contentCustomer.Replace("{{Ten}}", HttpUtility.HtmlEncode(customerName));
            contentCustomer = contentCustomer.Replace("{{DienThoai}}", HttpUtility.HtmlEncode(customerPhone));
            contentCustomer = contentCustomer.Replace("{{DiaChi}}", HttpUtility.HtmlEncode(customerAddress));
            contentCustomer = contentCustomer.Replace("{{ThanhToan}}", HttpUtility.HtmlEncode(DescribePaymentType(paymentType)));
            contentCustomer = contentCustomer.Replace("{{Time}}", (order != null ? order.createdDate : DateTime.Now).ToString("dd/MM/yyyy HH:mm"));
            contentCustomer = contentCustomer.Replace("{{TongTien}}", CommonHelper.ConvertMoney(tongtien));
            contentCustomer = contentCustomer.Replace("{{BaseUrl}}", HttpUtility.HtmlAttributeEncode(baseUrl));

            return contentCustomer;
        }

        private static string BuildOrderEmailRow(string baseUrl, string productImg, string productName, int price, int quantity, long thanhTien)
        {
            string imgUrl = productImg ?? string.Empty;
            if (!string.IsNullOrEmpty(imgUrl)
                && !imgUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !imgUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                && !imgUrl.StartsWith("//", StringComparison.Ordinal))
            {
                imgUrl = baseUrl + (imgUrl.StartsWith("/") ? imgUrl : "/" + imgUrl);
            }

            string safeName = HttpUtility.HtmlEncode(productName ?? string.Empty);
            string safeImg = HttpUtility.HtmlAttributeEncode(imgUrl);
            if (quantity < 1)
            {
                quantity = 1;
            }

            string imageCell = string.IsNullOrEmpty(imgUrl)
                ? string.Empty
                : string.Format(
                    "<td width=\"68\" valign=\"middle\" style=\"padding-right:12px;\">"
                    + "<img src=\"{0}\" alt=\"{1}\" width=\"56\" style=\"display:block;width:56px;max-width:56px;height:56px;object-fit:contain;background:#fafafa;border-radius:8px;border:1px solid #ececef;\" />"
                    + "</td>",
                    safeImg,
                    safeName);

            var row = new StringBuilder();
            row.Append("<tr><td style=\"padding:14px 0;border-bottom:1px solid #ececef;\">");
            row.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr>");
            row.Append(imageCell);
            row.Append("<td valign=\"middle\" style=\"font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;\">");
            row.AppendFormat("<div style=\"font-size:14px;font-weight:600;line-height:1.45;color:#18181b;\">{0}</div>", safeName);
            row.AppendFormat("<div style=\"margin-top:3px;font-size:12.5px;line-height:1.45;color:#6b7280;\">SL: {0} &times; {1} đ</div>", quantity, CommonHelper.ConvertMoney(price));
            row.Append("</td>");
            row.AppendFormat("<td valign=\"middle\" align=\"right\" width=\"120\" style=\"font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:14px;font-weight:600;color:#18181b;white-space:nowrap;\">{0} đ</td>", CommonHelper.ConvertMoney(thanhTien));
            row.Append("</tr></table>");
            row.Append("</td></tr>");
            return row.ToString();
        }

        private static string DescribePaymentType(int paymentType)
        {
            return PaymentConfigHelper.GetMethodTitle(paymentType);
        }

    }
}
