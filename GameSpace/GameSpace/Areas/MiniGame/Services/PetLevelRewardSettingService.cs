using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetLevelRewardSettingService : IPetLevelRewardSettingService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<PetLevelRewardSettingService> _logger;

        public PetLevelRewardSettingService(GameSpacedatabaseContext context, ILogger<PetLevelRewardSettingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PetLevelRewardSettingListViewModel>> GetAllAsync()
        {
            try
            {
                var settings = await _context.Set<PetLevelRewardSetting>()
                    .OrderBy(s => s.Level)
                    .ThenBy(s => s.RewardType)
                    .Select(s => new PetLevelRewardSettingListViewModel
                    {
                        Id = s.Id,
                        Level = s.Level,
                        RewardType = s.RewardType,
                        RewardAmount = s.RewardAmount,
                        Description = s.Description,
                        IsEnabled = s.IsEnabled,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CreatedBy = s.CreatedBy,
                        UpdatedBy = s.UpdatedBy
                    })
                    .ToListAsync();

                _logger.LogInformation("成功取得所有寵物升級獎勵設定，共 {Count} 筆", settings.Count());
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有寵物升級獎勵設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetLevelRewardSettingViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>()
                    .Where(s => s.Id == id)
                    .Select(s => new PetLevelRewardSettingViewModel
                    {
                        Id = s.Id,
                        Level = s.Level,
                        RewardType = s.RewardType,
                        RewardAmount = s.RewardAmount,
                        Description = s.Description,
                        IsEnabled = s.IsEnabled,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CreatedBy = s.CreatedBy,
                        UpdatedBy = s.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (setting != null)
                {
                    _logger.LogInformation("成功取得寵物升級獎勵設定 ID: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("找不到寵物升級獎勵設定 ID: {Id}", id);
                }

                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物升級獎勵設定 ID: {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<PetLevelRewardSettingViewModel?> CreateAsync(PetLevelRewardSettingCreateViewModel model)
        {
            try
            {
                // 驗證等級是否已存在
                if (!await ValidateLevelAsync(model.Level))
                {
                    _logger.LogWarning("等級 {Level} 已存在，無法建立重複的獎勵設定", model.Level);
                    return null;
                }

                var setting = new PetLevelRewardSetting
                {
                    Level = model.Level,
                    RewardType = model.RewardType,
                    RewardAmount = model.RewardAmount,
                    Description = model.Description,
                    IsEnabled = model.IsEnabled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System", // 實際應用中應從認證系統取得
                    UpdatedBy = "System"
                };

                _context.Set<PetLevelRewardSetting>().Add(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功建立寵物升級獎勵設定，等級: {Level}, 獎勵類型: {RewardType}", 
                    model.Level, model.RewardType);

                return new PetLevelRewardSettingViewModel
                {
                    Id = setting.Id,
                    Level = setting.Level,
                    RewardType = setting.RewardType,
                    RewardAmount = setting.RewardAmount,
                    Description = setting.Description,
                    IsEnabled = setting.IsEnabled,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt,
                    CreatedBy = setting.CreatedBy,
                    UpdatedBy = setting.UpdatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立寵物升級獎勵設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetLevelRewardSettingViewModel?> UpdateAsync(PetLevelRewardSettingEditViewModel model)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>().FindAsync(model.Id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到要更新的寵物升級獎勵設定 ID: {Id}", model.Id);
                    return null;
                }

                // 驗證等級是否已存在（排除自己）
                if (!await ValidateLevelAsync(model.Level, model.Id))
                {
                    _logger.LogWarning("等級 {Level} 已存在，無法更新", model.Level);
                    return null;
                }

                setting.Level = model.Level;
                setting.RewardType = model.RewardType;
                setting.RewardAmount = model.RewardAmount;
                setting.Description = model.Description;
                setting.IsEnabled = model.IsEnabled;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = "System"; // 實際應用中應從認證系統取得

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新寵物升級獎勵設定 ID: {Id}", model.Id);

                return new PetLevelRewardSettingViewModel
                {
                    Id = setting.Id,
                    Level = setting.Level,
                    RewardType = setting.RewardType,
                    RewardAmount = setting.RewardAmount,
                    Description = setting.Description,
                    IsEnabled = setting.IsEnabled,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt,
                    CreatedBy = setting.CreatedBy,
                    UpdatedBy = setting.UpdatedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物升級獎勵設定 ID: {Id} 時發生錯誤", model.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>().FindAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到要刪除的寵物升級獎勵設定 ID: {Id}", id);
                    return false;
                }

                _context.Set<PetLevelRewardSetting>().Remove(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除寵物升級獎勵設定 ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物升級獎勵設定 ID: {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            try
            {
                var setting = await _context.Set<PetLevelRewardSetting>().FindAsync(id);
                if (setting == null)
                {
                    _logger.LogWarning("找不到要切換狀態的寵物升級獎勵設定 ID: {Id}", id);
                    return false;
                }

                setting.IsEnabled = !setting.IsEnabled;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = "System"; // 實際應用中應從認證系統取得

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功切換寵物升級獎勵設定 ID: {Id} 狀態為: {IsEnabled}", 
                    id, setting.IsEnabled);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物升級獎勵設定 ID: {Id} 狀態時發生錯誤", id);
                throw;
            }
        }

        public async Task<IEnumerable<PetLevelRewardSettingListViewModel>> SearchAsync(PetLevelRewardSettingSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Set<PetLevelRewardSetting>().AsQueryable();

                if (searchModel.Level.HasValue)
                {
                    query = query.Where(s => s.Level == searchModel.Level.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.RewardType))
                {
                    query = query.Where(s => s.RewardType.Contains(searchModel.RewardType));
                }

                if (searchModel.IsEnabled.HasValue)
                {
                    query = query.Where(s => s.IsEnabled == searchModel.IsEnabled.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.CreatedBy))
                {
                    query = query.Where(s => s.CreatedBy != null && s.CreatedBy.Contains(searchModel.CreatedBy));
                }

                var settings = await query
                    .OrderBy(s => s.Level)
                    .ThenBy(s => s.RewardType)
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(s => new PetLevelRewardSettingListViewModel
                    {
                        Id = s.Id,
                        Level = s.Level,
                        RewardType = s.RewardType,
                        RewardAmount = s.RewardAmount,
                        Description = s.Description,
                        IsEnabled = s.IsEnabled,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CreatedBy = s.CreatedBy,
                        UpdatedBy = s.UpdatedBy
                    })
                    .ToListAsync();

                _logger.LogInformation("搜尋寵物升級獎勵設定完成，找到 {Count} 筆結果", settings.Count());
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋寵物升級獎勵設定時發生錯誤");
                throw;
            }
        }

        public async Task<(int TotalCount, int TotalPages)> GetPaginationInfoAsync(PetLevelRewardSettingSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Set<PetLevelRewardSetting>().AsQueryable();

                if (searchModel.Level.HasValue)
                {
                    query = query.Where(s => s.Level == searchModel.Level.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.RewardType))
                {
                    query = query.Where(s => s.RewardType.Contains(searchModel.RewardType));
                }

                if (searchModel.IsEnabled.HasValue)
                {
                    query = query.Where(s => s.IsEnabled == searchModel.IsEnabled.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.CreatedBy))
                {
                    query = query.Where(s => s.CreatedBy != null && s.CreatedBy.Contains(searchModel.CreatedBy));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / searchModel.PageSize);

                return (totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得分頁資訊時發生錯誤");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetStatisticsAsync()
        {
            try
            {
                var totalCount = await _context.Set<PetLevelRewardSetting>().CountAsync();
                var enabledCount = await _context.Set<PetLevelRewardSetting>().CountAsync(s => s.IsEnabled);
                var disabledCount = totalCount - enabledCount;

                var rewardTypeStats = await _context.Set<PetLevelRewardSetting>()
                    .GroupBy(s => s.RewardType)
                    .Select(g => new { RewardType = g.Key, Count = g.Count() })
                    .ToListAsync();

                var levelStats = await _context.Set<PetLevelRewardSetting>()
                    .GroupBy(s => s.Level)
                    .Select(g => new { Level = g.Key, Count = g.Count() })
                    .OrderBy(s => s.Level)
                    .ToListAsync();

                var statistics = new Dictionary<string, object>
                {
                    ["TotalCount"] = totalCount,
                    ["EnabledCount"] = enabledCount,
                    ["DisabledCount"] = disabledCount,
                    ["RewardTypeStats"] = rewardTypeStats,
                    ["LevelStats"] = levelStats
                };

                _logger.LogInformation("成功取得寵物升級獎勵設定統計資料");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得統計資料時發生錯誤");
                throw;
            }
        }

        public async Task<bool> ValidateLevelAsync(int level, int? excludeId = null)
        {
            try
            {
                var query = _context.Set<PetLevelRewardSetting>().Where(s => s.Level == level);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(s => s.Id != excludeId.Value);
                }

                var exists = await query.AnyAsync();
                return !exists; // 如果不存在則返回 true（可以建立）
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證等級 {Level} 時發生錯誤", level);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetRewardTypesAsync()
        {
            try
            {
                var rewardTypes = await _context.Set<PetLevelRewardSetting>()
                    .Select(s => s.RewardType)
                    .Distinct()
                    .OrderBy(rt => rt)
                    .ToListAsync();

                _logger.LogInformation("成功取得所有獎勵類型，共 {Count} 種", rewardTypes.Count);
                return rewardTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得獎勵類型時發生錯誤");
                throw;
            }
        }
    }
}

