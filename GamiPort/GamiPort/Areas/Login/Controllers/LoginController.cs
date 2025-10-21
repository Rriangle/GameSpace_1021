using GamiPort.Data;                     // ApplicationDbContext（Identity 用）
using GamiPort.Models;                   // GameSpacedatabaseContext（業務資料）
using GamiPort.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	public class LoginController : Controller
	{
		private readonly GameSpacedatabaseContext _bizDb;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;

		public LoginController(
			GameSpacedatabaseContext bizDb,
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager)
		{
			_bizDb = bizDb;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			return View(new LoginVM { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			// 1) 以你的 Users 表驗證帳密
			var user = await _bizDb.Users
				.Include(u => u.UserIntroduce)
				.FirstOrDefaultAsync(u => u.UserAccount == vm.UserAccount);

			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "帳號或密碼錯誤。");
				return View(vm);
			}

			// 密碼驗證 — 先用明碼；日後切到 BCrypt（註解在下面）
			var passwordOk = user.UserPassword == vm.UserPassword;

			// 建議：導入 BCrypt 後改用下列語句
			// var passwordOk = BCrypt.Net.BCrypt.Verify(vm.UserPassword, user.UserPassword);

			if (!passwordOk)
			{
				// TODO: 可在此累計 AccessFailedCount 與 Lockout 機制
				ModelState.AddModelError(string.Empty, "帳號或密碼錯誤。");
				return View(vm);
			}

			// 2) 檢查 / 建立對應的 IdentityUser
			var identityId = $"U:{user.UserId}";
			var identityUser = await _userManager.FindByIdAsync(identityId);
			if (identityUser == null)
			{
				identityUser = new IdentityUser
				{
					Id = identityId,
					UserName = user.UserName,
					Email = user.UserIntroduce?.Email, // 若沒有就留 null
					EmailConfirmed = user.UserEmailConfirmed
				};
				// 建一個隨機密碼（不會用到 PasswordSignIn）
				var tempPwd = $"Tmp{Guid.NewGuid():N}!aA1";
				var createResult = await _userManager.CreateAsync(identityUser, tempPwd);
				if (!createResult.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "建立登入資訊失敗。");
					return View(vm);
				}
			}

			// 3) 用 SignInManager 登入（把你的 UserId 一併放到 Claims）
			var claims = new List<Claim>
			{
				new Claim("AppUserId", user.UserId.ToString()), // 自家 UserId（數字）
                new Claim(ClaimTypes.Name, user.UserName)        // 顯示用途
            };

			await _signInManager.SignOutAsync(); // 保險：避免殘留舊登入
			await _signInManager.SignInWithClaimsAsync(identityUser, isPersistent: vm.RememberMe, claims);

			if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
				return Redirect(vm.ReturnUrl);

			return RedirectToAction("Index", "Home", new { area = "" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize] // 已登入者才需要登出
		public async Task<IActionResult> Logout(string? returnUrl = null)
		{
			await _signInManager.SignOutAsync();                 // 清掉 GamiPort.User

			// 若同時有其他 Cookie/外部登入，可視需要一併清：
			// await HttpContext.SignOutAsync("AdminCookie");
			// await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			// HttpContext.Session?.Clear();

			return Redirect(returnUrl ?? Url.Action("Index", "Home", new { area = "" })!);
		}

		// AccessDeniedPath 導向的頁
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Denied() => View();
	}
}
