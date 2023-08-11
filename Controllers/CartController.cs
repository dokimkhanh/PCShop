using PCShop.Models;
using PCShop.Models.Common;
using PCShop.Models.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Controllers
{
    public class CartController : Controller
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        // GET: Cart
        public ActionResult Index()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart != null && cart.listCartItem.Any())
            {
                return View(cart.listCartItem);
            }
            return View();
        }

        public ActionResult CheckOut()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart != null && cart.listCartItem.Any())
            {
                return View(cart.listCartItem);
            }
            return RedirectToAction("Index", "Cart");
        }

        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                string orderCode = vnpay.GetResponseData("vnp_TxnRef").ToString();
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var item = dbContext.orders.Where(x => x.code == orderCode).FirstOrDefault();
                        if (item != null)
                        {
                            item.paymentStatus = 1;
                            dbContext.orders.Attach(item);
                            dbContext.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                    ViewBag.Total = vnp_Amount;
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
                else
                {
                    //log.InfoFormat("Invalid signature, InputData={0}", Request.RawUrl);
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(Order order)
        {
            var code = new { Success = false, Code = -1, Url = "" };
            if (ModelState.IsValid)
            {
                Cart cart = (Cart)Session["Cart"];
                if (cart != null)
                {
                    order.code = "PCS" + CommonHelper.RandomNumber(9);
                    order.totalAmount = cart.listCartItem.Sum(x => (x.quantity * x.price));

                    cart.listCartItem.ForEach(x => order.orderDetails.Add(new OrderDetail
                    {
                        productID = x.Id,
                        quantity = x.quantity,
                        price = x.price
                    }));

                    order.paymentStatus = 0; //0: Pending; 1: Completed; 2: Canceled
                    order.createdDate = DateTime.Now;
                    order.createdBy = User.Identity.Name;
                    order.modifiedDate = DateTime.Now;

                    dbContext.orders.Add(order);
                    dbContext.SaveChanges();

                    var strSanPham = "";
                    int tongtien = 0;
                    foreach (var item in cart.listCartItem)
                    {
                        strSanPham += $@"<tr style=""border-bottom: 1px solid rgba(0,0,0,.05);"">
                            <td valign=""middle"" width=""80%"" style=""text-align:left; padding: 0 2.5em;"">
                                <div class=""product-entry"">
                                    <img src=""{Request.Url.Host}{item.productImg}"" alt=""{item.productName}"" style=""width: 100px; max-width: 600px; height: auto; margin-bottom: 20px; display: block;"">
                                    <div class=""text"">
                                        <h3>{item.productName}</h3>
                                        <p>{CommonHelper.ConvertMoney(item.price)}đ</p>
                                    </div>
                                </div>
                            </td>
                            <td valign=""middle"" width=""20%"" style=""text-align:left; padding: 0 2.5em;"">
                                <span class=""price"" style=""color: #000; font-size: 20px;"">{CommonHelper.ConvertMoney(item.totalPrice)}đ</span>
                            </td>
                        </tr>";
                        tongtien = item.price * item.quantity;
                    }
                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/template/mail1.html"));
                    contentCustomer = contentCustomer.Replace("{{MaDonHang}}", order.code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{Ten}}", order.customerName);
                    contentCustomer = contentCustomer.Replace("{{DiaChi}}", order.address);
                    contentCustomer = contentCustomer.Replace("{{Time}}", DateTime.Now.ToString());
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", CommonHelper.ConvertMoney(tongtien));

                    MailHelper.SendMail("PCShop", "ĐƠN HÀNG " + order.code, contentCustomer, order.email);
                    cart.ClearCart();

                    if (order.paymentType != 10)
                    {
                        var url = UrlPayment(order.paymentType, order.code);
                        code = new { Success = true, Code = 1, Url = url };

                    } else
                    {
                        code = new { Success = true, Code = 2, Url = "" };
                    }
                }
            }
            return Json(code);
        }

        public ActionResult CheckOutSuccess()
        {
            return View();
        }

        public ActionResult ShowCount()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.GetTotalQuantity() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AddItem(int id, int quantity)
        {
            var code = new { Success = false, msg = "Không thể thêm sản phẩm", Code = -1 };
            var checkProduct = dbContext.products.FirstOrDefault(x => x.id == id);
            if (checkProduct != null)
            {
                Cart cart = (Cart)Session["Cart"];
                if (cart == null)
                {
                    cart = new Cart();
                }
                CartItems cartItems = new CartItems
                {
                    Id = checkProduct.id,
                    productName = checkProduct.title,
                    Alias = checkProduct.alias,
                    categoryName = checkProduct.productCategory.title,
                    price = checkProduct.price,
                    quantity = quantity
                };
                if (checkProduct.productImages.FirstOrDefault(x => x.isDefault) != null)
                {
                    cartItems.productImg = checkProduct.productImages.FirstOrDefault(x => x.isDefault).image;
                }
                if (checkProduct.priceSale > 0)
                {
                    cartItems.price = checkProduct.priceSale;
                }
                cartItems.totalPrice = cartItems.price * cartItems.quantity;
                cart.AddItem(cartItems, quantity);
                Session["Cart"] = cart;
                code = new { Success = true, msg = "Thêm sản phẩm thành công", Code = 1 };
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            var code = new { Success = false, msg = "Không thể xoá sản phẩm", Code = -1 };

            Cart cart = (Cart)Session["Cart"];
            if (cart != null)
            {
                var checkCart = cart.listCartItem.FirstOrDefault(x => x.Id == id);
                if (checkCart != null)
                {
                    cart.DeleteItem(id);
                    code = new { Success = true, msg = "Đã xoá sản phẩm khỏi giỏ hàng", Code = 1 };

                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult ClearCart()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        public ActionResult UpdateQuantityCartItem(int id, int quantity)
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        public string UrlPayment(int TypePaymentVN, string OrderCode)
        {
            string paymentUrl = "";
            var order = dbContext.orders.FirstOrDefault(x => x.code == OrderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (order.totalAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
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
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.code); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;

        }

    }
}