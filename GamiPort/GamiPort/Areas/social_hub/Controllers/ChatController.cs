using GamiPort.Infrastructure.Login;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public sealed class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ILoginIdentity _login;

		public ChatController(GameSpacedatabaseContext db, ILoginIdentity login)
		{ _db = db; _login = login; }

		private async Task<int> GetMeAsync()
		{
			var id = await _login.GetAsync();
			return id.IsAuthenticated && id.UserId is > 0 ? id.UserId.Value : 0;
		}

		private Task<bool> ExistsAsync(int userId)
			// 若你的屬性是 User_ID，改成 u => u.User_ID == userId
			=> _db.Users.AnyAsync(u => u.UserId == userId);

		public async Task<IActionResult> With(int otherId)
		{
			var me = await GetMeAsync();
			if (me <= 0) return Content("請用 ?asUser=10000001 設定測試用戶，或完成登入。");
			if (otherId <= 0 || otherId == me) return BadRequest("otherId 不合法");

			if (!await ExistsAsync(me) || !await ExistsAsync(otherId)) return NotFound("USER_NOT_FOUND");
			ViewBag.MeId = me;
			ViewBag.OtherId = otherId;
			return View(); // 你可套用自己的 View；或先放空白，再用 Hub 互動
		}

		[HttpGet]
		public async Task<IActionResult> History(int otherId, string? afterIso)
		{
			var me = await GetMeAsync();
			if (me <= 0) return Unauthorized();
			if (!await ExistsAsync(me) || !await ExistsAsync(otherId)) return NotFound();

			DateTime? after = null;
			if (!string.IsNullOrWhiteSpace(afterIso) &&
				DateTime.TryParse(afterIso, null, DateTimeStyles.RoundtripKind, out var t)) after = t;

			var p1 = Math.Min(me, otherId);
			var p2 = Math.Max(me, otherId);
			var conv = await _db.DmConversations.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);
			if (conv == null) return Json(Array.Empty<object>());

			var iAmP1 = (me == conv.Party1Id);
			var q = _db.DmMessages.Where(m => m.ConversationId == conv.ConversationId);
			if (after.HasValue) q = q.Where(m => m.EditedAt > after.Value);

			var list = await q.OrderBy(m => m.EditedAt).Select(m => new {
				MessageId = m.MessageId,
				SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
				ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
				Content = m.MessageText,
				SentAtIso = m.EditedAt.ToString("o"),
				IsMine = (m.SenderIsParty1 == iAmP1),
				IsRead = m.IsRead
			}).ToListAsync();

			return Json(list);
		}
	}
}
