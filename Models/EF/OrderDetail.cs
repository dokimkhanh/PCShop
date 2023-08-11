using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PCShop.Models.EF
{
    [Table("tbOrderDetail")]
    public class OrderDetail
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int orderID { get; set; }
        public int productID { get; set; }
        public int price { get; set; }
        public int quantity { get; set; }

        public virtual Order order { get; set; }
        public virtual Product product { get; set; }
    }
}