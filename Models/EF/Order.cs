using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace PCShop.Models.EF
{
    [Table("tbOrder")]
    public class Order : AbsCommon
    {
        public Order()
        {
            this.orderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string customerName { get; set; }
        [Required(ErrorMessage = "Thiếu số điện đoạn liên hệ")]
        public string phone { get; set; }
        [Required(ErrorMessage = "Thiếu địa chỉ giao hàng")]
        public string address { get; set; }
        public string email { get; set; }
        public int totalAmount { get; set; }
        public int quantity { get; set; }
        public int paymentStatus { get; set; }
        public int paymentType { get; set; }
        public virtual ICollection<OrderDetail> orderDetails { get; set; }
    }
}