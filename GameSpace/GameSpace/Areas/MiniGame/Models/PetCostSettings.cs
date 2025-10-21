using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物換色所需點數設定
    /// ⚠️ 修復：移除 Table 屬性，使用 InMemory 服務替代資料庫
    /// </summary>
    public class PetSkinColorCostSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [StringLength(7)]
        public string ColorCode { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int RequiredPoints { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 寵物換背景所需點數設定
    /// </summary>
    [Table("PetBackgroundCostSetting")]
    public class PetBackgroundCostSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BackgroundName { get; set; } = string.Empty;

        [Required]
        [StringLength(7)]
        public string BackgroundCode { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int RequiredPoints { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}

