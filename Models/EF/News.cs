using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCShop.Models.EF
{
    [Table("News")]
    public class News : AbsCommon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required(ErrorMessage = "Tiêu đề không được bỏ trống")]
        public string title { get; set; }
        public string alias { get; set; }
        [Required(ErrorMessage = "Mô tả không được bỏ trống")]
        public string description { get; set; }
        [AllowHtml]
        [Required(ErrorMessage = "Nội dung chi tiết không được bỏ trống")]
        public string detail { get; set; }
        [Required(ErrorMessage = "Thiếu ảnh bìa")]
        public string image { get; set; }

    }
}