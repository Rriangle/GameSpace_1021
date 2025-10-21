using System.Security.Claims;
using GamiPort.Models;
using GamiPort.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace GamiPort.ViewComponents
{
	public class TopbarLevel1ViewComponent : ViewComponent
	{
		private readonly GameSpacedatabaseContext _bizDb;
		public TopbarLevel1ViewComponent(GameSpacedatabaseContext bizDb)
		{
			_bizDb = bizDb;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var vm = new TopbarVM();

			if (User?.Identity?.IsAuthenticated ?? false)
			{
				var idStr = ((ClaimsPrincipal)User).FindFirst("AppUserId")?.Value;
				if (int.TryParse(idStr, out int userId))
				{
					vm.UserId = userId;
					vm.NickName = await _bizDb.UserIntroduces
						.Where(u => u.UserId == userId)
						.Select(u => u.UserNickName)
						.FirstOrDefaultAsync();
				}
			}

			return View("Default", vm);
		}
	}


}
