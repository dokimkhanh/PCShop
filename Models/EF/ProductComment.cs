using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models.EF
{
    [Table("ProductComment")]
    public class ProductComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Required]
        [StringLength(120)]
        [Column("author_name")]
        public string AuthorName { get; set; }

        [StringLength(180)]
        [Column("author_email")]
        public string AuthorEmail { get; set; }

        [StringLength(128)]
        [Column("user_id")]
        public string UserId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("is_admin_reply")]
        public bool IsAdminReply { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
