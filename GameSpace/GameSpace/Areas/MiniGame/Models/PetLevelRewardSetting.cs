using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物升級獎勵設定
    /// </summary>
    [Table("PetLevelRewardSettings")]
    public class PetLevelRewardSetting
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 等級
        /// </summary>
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }

        /// <summary>
        /// 獎勵類型
        /// </summary>
        [Required(ErrorMessage = "獎勵類型為必填欄位")]
        [StringLength(50, ErrorMessage = "獎勵類型長度不能超過50個字元")]
        [Display(Name = "獎勵類型")]
        public string RewardType { get; set; } = string.Empty;

        /// <summary>
        /// 獎勵數量
        /// </summary>
        [Required(ErrorMessage = "獎勵數量為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵數量必須大於等於0")]
        [Display(Name = "獎勵數量")]
        public int RewardAmount { get; set; }

        /// <summary>
        /// 獎勵描述
        /// </summary>
        [StringLength(200, ErrorMessage = "獎勵描述長度不能超過200個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立者
        /// </summary>
        [StringLength(50)]
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [StringLength(50)]
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
}

