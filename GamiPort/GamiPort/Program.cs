// =======================
// Program.cs（完整覆蓋版）
// 目的：支援 Dev 模式下以 Cookie 取得 UserId、SignalR 聊天、Areas/MVC/RazorPages 與 Identity
// =======================

using GamiPort.Data;                           // ApplicationDbContext（Identity / 登入資料）
using GamiPort.Models;                         // GameSpacedatabaseContext（業務資料：聊天、通知…）
using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Areas.social_hub.Services.Application;
using GamiPort.Infrastructure.Login;           // ILoginIdentity / DevCookieLoginIdentity / DevQueryLoginMiddleware
using GamiPort.Areas.social_hub.Hubs;          // ChatHub（SignalR Hub）
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GamiPort
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// 建立主機組態/DI 容器
			var builder = WebApplication.CreateBuilder(args);

			// ------------------------------------------------------------
			// 連線字串：兩個 DbContext 共用同一組；未來要分庫可再拆
			// 依序嘗試 "GameSpace" -> "GameSpacedatabase" -> 否則丟例外
			// ------------------------------------------------------------
			var gameSpaceConn =
				builder.Configuration.GetConnectionString("GameSpace")
				?? builder.Configuration.GetConnectionString("GameSpacedatabase")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");

			// ------------------------------------------------------------
			// MVC / RazorPages 服務（合併成一次註冊，並保留 JSON 選項）
			// AddControllersWithViews：提供傳統 MVC + Areas + Views
			// AddRazorPages：提供 Identity 預設 UI（/Identity/...）
			// ------------------------------------------------------------
			builder.Services
				.AddControllersWithViews()
				.AddJsonOptions(opt =>
				{
					// 統一大小寫；避免 API 自動小寫屬性名稱造成前端困惑
					opt.JsonSerializerOptions.PropertyNamingPolicy = null;
				});
			builder.Services.AddRazorPages();

			// ------------------------------------------------------------
			// (A) DbContext 註冊
			// 1) ApplicationDbContext：Identity（登入/使用者/Claims）
			// 2) GameSpacedatabaseContext：你的業務資料表（聊天、通知、客服…）
			// ------------------------------------------------------------
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
				// options.EnableSensitiveDataLogging(); // 可選：開發期除錯
			});

			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
			{
				options.UseSqlServer(gameSpaceConn);
				// options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // 可選：以查詢為主的頁面
			});

			// EF 開發者例外頁（顯示更完整的 Db 例外，含 /errors 與 migrations 支援）
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// ------------------------------------------------------------
			// (B) Identity 設定（若暫時不用也可先保留，之後無縫接）
			// AddDefaultIdentity 會配置 Cookie、SecurityStamp 驗證等
			// ------------------------------------------------------------
			builder.Services
				.AddDefaultIdentity<IdentityUser>(options =>
				{
					// 若暫時沒有寄信機制，開發環境建議關閉信箱驗證需求
					// options.SignIn.RequireConfirmedAccount = false;
					options.SignIn.RequireConfirmedAccount = false;
				})
				.AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.ConfigureApplicationCookie(opt =>
			{
				opt.LoginPath = "/Login/Login";
				opt.AccessDeniedPath = "/Login/Login/Denied";
				opt.Cookie.Name = "GamiPort.User";   // 與後台 AdminCookie 不同名，避免混用
													 // 視需求：opt.SlidingExpiration = true;
			});

			// 建議明確加入授權服務（供 [Authorize] 等使用）
			builder.Services.AddAuthorization();

			// ------------------------------------------------------------
			// (C) 專案服務註冊（通知、朋友關係等）
			// ------------------------------------------------------------
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<INotificationStore, NotificationStore>();
			builder.Services.AddScoped<IRelationService, RelationService>();

			// ------------------------------------------------------------
			// (D) 目前使用者來源（關鍵）：開發期用 Cookie 存 UserId
			// ILoginIdentity：全站統一從這裡取得 UserId
			// DevCookieLoginIdentity：優先讀 Cookie "gp_dev_uid"，否則讀 appsettings.Development.json:DevLogin:UserId
			// ------------------------------------------------------------
			builder.Services.AddHttpContextAccessor();                         // 讓服務可以讀取目前 HttpContext
			builder.Services.AddScoped<ILoginIdentity, DevCookieLoginIdentity>(); // 開發期的身分來源

			// ------------------------------------------------------------
			// (E) SignalR（聊天即時通訊）
			// ------------------------------------------------------------
			builder.Services.AddSignalR(); // 註冊 SignalR 服務（Hub 需要）

			// ------------------------------------------------------------
			// (F) Anti-forgery：若你在 Controller 有 [ValidateAntiForgeryToken]，前端可用此標頭傳遞
			// ------------------------------------------------------------
			builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

			// 建立應用程式
			var app = builder.Build();

			// ------------------------------------------------------------
			// HTTP Pipeline（中介軟體順序很重要）
			// ------------------------------------------------------------
			if (app.Environment.IsDevelopment())
			{
				// 顯示 EF 錯誤詳情頁、提供 migrations 端點
				app.UseMigrationsEndPoint();

				// （可選）啟動時檢查 DB 是否可連線，快速發現連線字串/權限問題
				using var scope = app.Services.CreateScope();
				var db1 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				var db2 = scope.ServiceProvider.GetRequiredService<GameSpacedatabaseContext>();
				db1.Database.CanConnect();
				db2.Database.CanConnect();

				// 開發期便利功能：支援網址 ?asUser=30000001 → 寫入 Cookie "gp_dev_uid"
				// 讓你不必做登入就能測聊天室
				app.UseDevQueryLoginParameter();
			}
			else
			{
				// 正式環境：例外處理頁 + HSTS
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			// 靜態檔、路由等基本中介軟體
			app.UseHttpsRedirection();  // 自動轉 https
			app.UseStaticFiles();       // 提供 wwwroot 內檔案
			app.UseRouting();           // 啟用端點路由

			// 身分驗證 / 授權（順序：先驗證再授權）
			app.UseAuthentication();
			app.UseAuthorization();

			// ------------------------------------------------------------
			// MVC 控制器端點（Attribute Routing 的控制器也會在這裡生效）
			// ------------------------------------------------------------
			app.MapControllers();

			// ------------------------------------------------------------
			// 傳統路由（先 Areas 再 default）
			// /{area}/{controller}/{action}/{id?}
			// ------------------------------------------------------------
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
			);

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}"
			);

			// Razor Pages（Identity 預設 UI 需要）
			app.MapRazorPages();

			// ------------------------------------------------------------
			// SignalR Hub 路由：前端會連線到 /social_hub/chatHub
			// 你的 _FloatingDock.cshtml / chat-dock.js 會連這個位址
			// ------------------------------------------------------------
			app.MapHub<ChatHub>("/social_hub/chatHub");

			// 啟動應用
			app.Run();
		}
	}
}
