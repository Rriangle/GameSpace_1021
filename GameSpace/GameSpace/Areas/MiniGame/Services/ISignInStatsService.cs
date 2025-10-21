using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface ISignInStatsService
    {
        Task<GameSpace.Areas.MiniGame.Models.ViewModels.PagedResult<UserSignInStats>> GetSignInStatsAsync(SignInStatsQueryModel query);
        Task<SignInStatsSummary> GetSignInStatsSummaryAsync();
        Task<bool> ConfigureSignInRulesAsync(SignInRulesModel rules);
        Task<SignInRulesModel> GetSignInRulesAsync();
    }
}



