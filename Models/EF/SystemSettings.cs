using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models.EF
{
    [Table("SystemSettings")]
    public class SystemSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã cấu hình.")]
        [StringLength(100)]
        public string settingKey { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên hiển thị.")]
        [StringLength(200)]
        public string settingName { get; set; }

        [StringLength(100)]
        public string settingGroup { get; set; }

        [StringLength(50)]
        public string settingType { get; set; }

        public string settingValue { get; set; }

        public string settingDescription { get; set; }
    }
}
