using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Models.EF
{
    [Table("Product")]
    public class Product : AbsCommon
    {
        public Product() {
            this.productImages = new HashSet<ProductImage>();
            this.orderDetail = new HashSet<OrderDetail>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required(ErrorMessage ="Thiếu tên sản phẩm")]
        public string title { get; set; }
        public string alias { get; set; }
        [Required(ErrorMessage = "Thiếu mã sản phẩm")]
        public string productCode { get; set; }
        public string description { get; set; }
        [AllowHtml]
        public string detail { get; set; }
        public string image { get; set; }
        public int price { get; set; }
        public int priceSale { get; set; }
        public int quantity { get; set; }
        public int categoryID { get; set; }
        public bool isNew { get; set; }
        public bool isSale { get; set; }
        public virtual Category productCategory { get; set;}
        public virtual ICollection<ProductImage> productImages { get; set; }
        public virtual ICollection<OrderDetail> orderDetail { get; set; }

    }
}