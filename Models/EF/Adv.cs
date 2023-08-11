using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PCShop.Models.EF
{
    [Table("Adv")]
    public class Adv : AbsCommon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string title { get; set;}
        public string description { get; set;}
        public string image { get; set;}
        public int type { get; set;}
        public string link { get; set;}
    }
}