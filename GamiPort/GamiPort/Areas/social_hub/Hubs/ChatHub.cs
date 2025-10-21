using System.Globalization;
using GamiPort.Infrastructure.Login;
using GamiPort.Models;                     // GameSpacedatabaseContext + DmConversation/DmMessage + Users
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.social_hub.Hubs
{
	public sealed class ChatHub : Hub
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ILoginIdentity _login;

		public ChatHub(GameSpacedatabaseContext db, ILoginIdentity login)
		{ _db = db; _login = login; }

		private static string UserGroup(int userId) => $"u:{userId}";

		public override async Task OnConnectedAsync()
		{
			var me = await GetMeAsync();
			if (me > 0) await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(me));
			await base.OnConnectedAsync();
		}
		public override async Task OnDisconnectedAsync(Exception? ex)
		{
			var me = await GetMeAsync();
			if (me > 0) await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(me));
			await base.OnDisconnectedAsync(ex);
		}

		public async Task<object> SendMessageTo(int otherId, string text)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException("NOT_LOGGED_IN");
			if (otherId <= 0 || otherId == me) throw new HubException("NO_PEER");

			// 驗證 sender / receiver 都存在（只看 ID）
			await EnsureUserExistsAsync(me);
			await EnsureUserExistsAsync(otherId);

			if (string.IsNullOrWhiteSpace(text)) throw new HubException("BAD_TEXT");
			text = text.Trim();
			if (text.Length > 255) text = text[..255];

			var conv = await GetOrCreateConversationAsync(me, otherId);
			var iAmP1 = (me == conv.Party1Id);
			var now = DateTime.UtcNow;

			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = text,
				IsRead = false,
				EditedAt = now
			};
			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync();

			var payload = new
			{
				messageId = msg.MessageId,
				senderId = me,
				receiverId = otherId,
				content = text, // 目前不做遮罩
				sentAtIso = now.ToString("o", CultureInfo.InvariantCulture)
			};

			await Clients.Group(UserGroup(me)).SendAsync("ReceiveDirect", payload);
			await Clients.Group(UserGroup(otherId)).SendAsync("ReceiveDirect", payload);

			await BroadcastUnreadAsync(me, otherId);
			await BroadcastUnreadAsync(otherId, me);
			return payload;
		}

		public async Task<object> NotifyRead(int otherId, string upToIso)
		{
			var me = await GetMeAsync();
			if (me <= 0) throw new HubException("NOT_LOGGED_IN");
			if (otherId <= 0 || otherId == me) throw new HubException("NO_PEER");

			await EnsureUserExistsAsync(me);
			await EnsureUserExistsAsync(otherId);

			if (!DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out var upToUtc))
				upToUtc = DateTime.UtcNow;

			var conv = await GetOrCreateConversationAsync(me, otherId);
			var iAmP1 = (me == conv.Party1Id);

			var q = _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId
						 && !m.IsRead
						 && m.SenderIsParty1 != iAmP1
						 && m.EditedAt <= upToUtc);

			await q.ExecuteUpdateAsync(s => s
				.SetProperty(m => m.IsRead, true)
				.SetProperty(m => m.ReadAt, DateTime.UtcNow));

			var payload = new { fromUserId = me, upToIso = upToUtc.ToString("o", CultureInfo.InvariantCulture) };
			await Clients.Group(UserGroup(me)).SendAsync("ReadReceipt", payload);
			await Clients.Group(UserGroup(otherId)).SendAsync("ReadReceipt", payload);

			await BroadcastUnreadAsync(me, otherId);
			await BroadcastUnreadAsync(otherId, me);
			return payload;
		}

		// ------- helpers -------
		private async Task<int> GetMeAsync()
		{
			var id = await _login.GetAsync();
			return id.IsAuthenticated && id.UserId is > 0 ? id.UserId.Value : 0;
		}

		private async Task EnsureUserExistsAsync(int userId)
		{
			// ★ 說明：此處假設 DbSet 名為 Users，主鍵屬性為 UserId
			// 若你的 EF 實體屬性是 User_ID，請將 u.UserId 換為 u.User_ID
			var exists = await _db.Users.AnyAsync(u => u.UserId == userId);
			if (!exists) throw new HubException("USER_NOT_FOUND");
		}

		private async Task<DmConversation> GetOrCreateConversationAsync(int a, int b)
		{
			var p1 = Math.Min(a, b);
			var p2 = Math.Max(a, b);

			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);

			if (conv != null) return conv;

			conv = new DmConversation
			{
				IsManagerDm = false,
				Party1Id = p1,
				Party2Id = p2,
				CreatedAt = DateTime.UtcNow
			};
			_db.DmConversations.Add(conv);
			await _db.SaveChangesAsync();
			return conv;
		}

		private async Task<(int total, int peer)> ComputeUnreadAsync(int userId, int peerId)
		{
			var conv = await GetOrCreateConversationAsync(userId, peerId);
			var iAmP1 = (userId == conv.Party1Id);

			var total = await _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
				.CountAsync();

			return (total, total);
		}

		private async Task BroadcastUnreadAsync(int userId, int peerId)
		{
			var (total, peer) = await ComputeUnreadAsync(userId, peerId);
			await Clients.Group(UserGroup(userId)).SendAsync("UnreadUpdate", new { total, peerId, unread = peer });
		}
	}
}
