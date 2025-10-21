using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GamiPort.Infrastructure.Login
{
	public sealed class DevCookieLoginIdentity : ILoginIdentity
	{
		public const string CookieName = "gp_dev_uid";
		private readonly IHttpContextAccessor _http;
		private readonly IConfiguration _config;
		public DevCookieLoginIdentity(IHttpContextAccessor http, IConfiguration config)
		{ _http = http; _config = config; }

		public Task<LoginIdentityResult> GetAsync(CancellationToken ct = default)
		{
			var ctx = _http.HttpContext;

			if (ctx?.Request.Cookies.TryGetValue(CookieName, out var raw) == true &&
				int.TryParse(raw, out var uid) && uid > 0)
			{
				return Task.FromResult(new LoginIdentityResult { IsAuthenticated = true, UserId = uid, Source = $"Cookie:{CookieName}" });
			}

			var confVal = _config["DevLogin:UserId"];
			if (int.TryParse(confVal, out var devUid) && devUid > 0)
			{
				return Task.FromResult(new LoginIdentityResult { IsAuthenticated = true, UserId = devUid, Source = "AppSettings:DevLogin:UserId" });
			}

			return Task.FromResult(new LoginIdentityResult { IsAuthenticated = false, UserId = null, Source = "None" });
		}
	}
}
