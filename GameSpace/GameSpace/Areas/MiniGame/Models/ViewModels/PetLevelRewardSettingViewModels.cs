using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    public class PetLevelRewardSettingViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "獎勵類型為必填欄位")]
        [StringLength(50, ErrorMessage = "獎勵類型長度不能超過50個字元")]
        [Display(Name = "獎勵類型")]
        public string RewardType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "獎勵數量為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵數量必須大於等於0")]
        [Display(Name = "獎勵數量")]
        public int RewardAmount { get; set; }
        
        [StringLength(200, ErrorMessage = "獎勵描述長度不能超過200個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
        
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }
        
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }
        
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
    
    public class PetLevelRewardSettingCreateViewModel
    {
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "獎勵類型為必填欄位")]
        [StringLength(50, ErrorMessage = "獎勵類型長度不能超過50個字元")]
        [Display(Name = "獎勵類型")]
        public string RewardType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "獎勵數量為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵數量必須大於等於0")]
        [Display(Name = "獎勵數量")]
        public int RewardAmount { get; set; }
        
        [StringLength(200, ErrorMessage = "獎勵描述長度不能超過200個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
    }
    
    public class PetLevelRewardSettingEditViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "獎勵類型為必填欄位")]
        [StringLength(50, ErrorMessage = "獎勵類型長度不能超過50個字元")]
        [Display(Name = "獎勵類型")]
        public string RewardType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "獎勵數量為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "獎勵數量必須大於等於0")]
        [Display(Name = "獎勵數量")]
        public int RewardAmount { get; set; }
        
        [StringLength(200, ErrorMessage = "獎勵描述長度不能超過200個字元")]
        [Display(Name = "獎勵描述")]
        public string? Description { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
    }
    
    public class PetLevelRewardSettingListViewModel
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public int RewardAmount { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
    
    public class PetLevelRewardSettingSearchViewModel
    {
        [Display(Name = "等級")]
        public int? Level { get; set; }
        
        [Display(Name = "獎勵類型")]
        public string? RewardType { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool? IsEnabled { get; set; }
        
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }
        
        [Display(Name = "頁碼")]
        public int Page { get; set; } = 1;
        
        [Display(Name = "每頁筆數")]
        public int PageSize { get; set; } = 10;
    }
}

