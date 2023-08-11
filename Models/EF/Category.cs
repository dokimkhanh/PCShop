using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PCShop.Models.EF
{
    [Table("Category")]
    public class Category : AbsCommon
    {
        public Category()
        {
            this.products = new HashSet<Product>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required(ErrorMessage = "Tên danh mục không được bỏ trống")]
        [StringLength(150)]
        public string title { get; set; }
        public string description { get; set; }

        public string alias { get; set; }

        public virtual ICollection<Product> products { get; set;}

    }
}