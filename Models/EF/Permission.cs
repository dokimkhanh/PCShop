using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models.EF
{
    [Table("Permissions")]
    public class Permission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã quyền.")]
        [StringLength(100)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên quyền.")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phân hệ.")]
        [StringLength(100)]
        public string Module { get; set; }

        [StringLength(300)]
        public string Description { get; set; }
    }
}
