using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace GamiPort.Infrastructure.Login
{
	public sealed class DevQueryLoginMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IWebHostEnvironment _env;
		public DevQueryLoginMiddleware(RequestDelegate next, IWebHostEnvironment env)
		{ _next = next; _env = env; }

		public async Task Invoke(HttpContext context)
		{
			if (_env.IsDevelopment() &&
				context.Request.Query.TryGetValue("asUser", out var v) &&
				int.TryParse(v.ToString(), out var uid) && uid > 0)
			{
				context.Response.Cookies.Append(
					DevCookieLoginIdentity.CookieName,
					uid.ToString(),
					new CookieOptions { Path = "/", HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps }
				);
			}
			await _next(context);
		}
	}

	public static class DevQueryLoginMiddlewareExtensions
	{
		public static IApplicationBuilder UseDevQueryLoginParameter(this IApplicationBuilder app)
			=> app.UseMiddleware<DevQueryLoginMiddleware>();
	}
}
