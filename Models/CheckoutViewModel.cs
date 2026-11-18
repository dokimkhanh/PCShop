using System.ComponentModel.DataAnnotations;

namespace PCShop.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        [StringLength(200, ErrorMessage = "Tên khách hàng không được vượt quá 200 ký tự")]
        [Display(Name = "Họ và tên")]
        public string customerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email để nhận hóa đơn điện tử")]
        [StringLength(254, ErrorMessage = "Email không được vượt quá 254 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public int paymentType { get; set; }
    }
}
