using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("SignInRule")]
    public partial class SignInRule
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public int SignInDay { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        public int Experience { get; set; }

        [Required]
        public bool HasCoupon { get; set; }

        [MaxLength(50)]
        public string? CouponTypeCode { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedBy { get; set; }

        [MaxLength(500)]
        public string? DeleteReason { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CouponTypeCode))]
        public virtual CouponType? CouponType { get; set; }

        [ForeignKey(nameof(DeletedBy))]
        public virtual ManagerDatum? DeletedByManager { get; set; }
    }
}
