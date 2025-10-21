using System;
using System.Threading;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions; // RelationCommand / RelationResult / IRelationService
using GamiPort.Models;                                  // GameSpacedatabaseContext / Relation / User
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 交友服務（集中驗證 + 狀態轉換；通過才寫入 DB）。
	///
	/// 設計重點：
	///  1) 以「狀態 ID」為主寫 DB（穩定、直觀）。
	///  2) 對稱關係唯一化：用 (UserIdSmall, UserIdLarge) = (min, max) 正規化。
	///  3) No-Op：沒有實際變更則回 NoOp，避免重複通知或無謂寫入。
	///  4) RequestedBy 只在 PENDING 有意義；非 PENDING 必為 NULL。
	///
	/// 狀態機（依你的 DB 實際 ID 調整常數）：
	///   NONE(6)：無關係（解除好友/解除封鎖後的自然終點）
	///   PENDING(1) ← friend_request（從 NONE / REJECTED / REMOVED）
	///   PENDING(1) → ACCEPTED(2)  ：accept（僅受邀方）
	///   PENDING(1) → REJECTED(5)  ：reject（僅受邀方）
	///   PENDING(1) → REMOVED(4)   ：cancel_request（僅邀請方；表示邀請被取消的歷史痕跡）
	///   *         → BLOCKED(3)    ：block（任一狀態可封鎖）
	///   BLOCKED(3) → NONE(6)      ：unblock（解除封鎖 → 無關係）
	///   ACCEPTED(2) → NONE(6)     ：unfriend（解除好友 → 無關係）
	///   ACCEPTED(2) → set_nickname：允許設定/清空暱稱（不改變狀態）
	///
	/// 互邀策略：
	///   預設 AUTO_ACCEPT_MUTUAL_REQUEST = false（安全）；若對方已送邀請，friend_request 會回 Fail 提醒用 accept。
	///   若改 true，則互邀直接升級為 ACCEPTED。
	/// </summary>
	public sealed class RelationService : IRelationService
	{
		private readonly GameSpacedatabaseContext _db;
		public RelationService(GameSpacedatabaseContext db) => _db = db;

		// ===== 狀態常數（請依資料表 Relation_Status 的實際 ID 調整）=====================
		private const int STATUS_PENDING = 1; // 待確認
		private const int STATUS_ACCEPTED = 2; // 已成為好友
		private const int STATUS_BLOCKED = 3; // 已封鎖
		private const int STATUS_REMOVED = 4; // 已移除（取消邀請的歷史標記）
		private const int STATUS_REJECTED = 5; // 已拒絕
		private const int STATUS_NONE = 6; // 無關係（解除好友/解除封鎖後的落點）

		// ===== 行為開關 ===============================================================
		private const bool AUTO_ACCEPT_MUTUAL_REQUEST = false;

		// ===== 其它驗證參數（依你的模型而定）===========================================
		private const int NICKNAME_MAXLEN = 10; // OnModelCreating 有 HasMaxLength(10)

		public async Task<RelationResult> ExecuteAsync(RelationCommand cmd, CancellationToken ct = default)
		{
			// ---- 0) 基本輸入驗證 ----------------------------------------------------
			var actor = cmd.ActorUserId;
			var target = cmd.TargetUserId;
			if (actor <= 0 || target <= 0) return Fail("UserId 不合法。");
			if (actor == target) return Fail("不可對自己操作。");
			if (string.IsNullOrWhiteSpace(cmd.ActionCode)) return Fail("ActionCode 必填。");

			// 使用者存在性（可依需要移除以提速）
			var hasActor = await _db.Users.AsNoTracking().AnyAsync(u => u.UserId == actor, ct);
			var hasTarget = await _db.Users.AsNoTracking().AnyAsync(u => u.UserId == target, ct);
			if (!hasActor || !hasTarget) return Fail("使用者不存在。");

			// ---- 1) pair 正規化 -----------------------------------------------------
			var small = Math.Min(actor, target);
			var large = Math.Max(actor, target);

			var row = await _db.Relations
				.SingleOrDefaultAsync(r => r.UserIdSmall == small && r.UserIdLarge == large, ct);

			// ---- 2) 動作路由 ---------------------------------------------------------
			var action = cmd.ActionCode.Trim().ToLowerInvariant();

			return action switch
			{
				"friend_request" or "request" => await DoFriendRequest(row, small, large, actor, ct),
				"accept" => await DoAccept(row, actor, ct),
				"reject" => await DoReject(row, actor, ct),
				"cancel_request" or "cancel" => await DoCancel(row, actor, ct),
				"block" => await DoBlock(row, small, large, actor, ct),
				"unblock" => await DoUnblock(row, actor, ct),   // BLOCKED → NONE
				"unfriend" or "remove_friend" or "delete_friend"
															 => await DoUnfriend(row, actor, ct),  // ACCEPTED → NONE
				"set_nickname" => await DoSetNickname(row, cmd.Nickname, ct),
				_ => Fail("未知的 ActionCode。")
			};
		}

		// ============================================================================
		// 個別動作實作
		// ============================================================================

		private async Task<RelationResult> DoFriendRequest(Relation? row, int small, int large, int actor, CancellationToken ct)
		{
			// 不存在 → 直接建立 PENDING
			if (row is null)
			{
				row = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = STATUS_PENDING,
					RequestedBy = actor,
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(row);
				await _db.SaveChangesAsync(ct);
				return Ok(row.RelationId, STATUS_PENDING, "pending");
			}

			// 封鎖 → 不可送邀請
			if (row.StatusId == STATUS_BLOCKED)
				return Fail("對方已封鎖或關係被封鎖。");

			// 已是好友 → 不降級；NoOp
			if (row.StatusId == STATUS_ACCEPTED)
				return NoOp(row.RelationId, STATUS_ACCEPTED, "accepted");

			// 目前為 PENDING
			if (row.StatusId == STATUS_PENDING)
			{
				if (row.RequestedBy == actor)
					return NoOp(row.RelationId, STATUS_PENDING, "pending");

				if (AUTO_ACCEPT_MUTUAL_REQUEST)
				{
					row.StatusId = STATUS_ACCEPTED;
					row.RequestedBy = null; // 離開 PENDING，置空
					row.UpdatedAt = DateTime.UtcNow;
					await _db.SaveChangesAsync(ct);
					return Ok(row.RelationId, STATUS_ACCEPTED, "accepted");
				}
				else
				{
					return Fail("對方已發出邀請，請改用 accept。");
				}
			}

			// 允許重新邀請：REJECTED / REMOVED / NONE → PENDING
			if (row.StatusId is STATUS_REJECTED or STATUS_REMOVED or STATUS_NONE)
			{
				row.StatusId = STATUS_PENDING;
				row.RequestedBy = actor;
				row.UpdatedAt = DateTime.UtcNow;
				await _db.SaveChangesAsync(ct);
				return Ok(row.RelationId, STATUS_PENDING, "pending");
			}

			return Fail("目前狀態不允許送出邀請。");
		}

		private async Task<RelationResult> DoAccept(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_PENDING) return Fail("不是待確認狀態。");
			if (row.RequestedBy == actor) return Fail("邀請方不得自行接受。");

			row.StatusId = STATUS_ACCEPTED;
			row.RequestedBy = null; // 離開 PENDING，置空
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_ACCEPTED, "accepted");
		}

		private async Task<RelationResult> DoReject(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_PENDING) return Fail("不是待確認狀態。");
			if (row.RequestedBy == actor) return Fail("邀請方不得拒絕。");

			row.StatusId = STATUS_REJECTED;
			row.RequestedBy = null; // 離開 PENDING，置空
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_REJECTED, "rejected");
		}

		private async Task<RelationResult> DoCancel(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_PENDING) return Fail("不是待確認狀態。");
			if (row.RequestedBy != actor) return Fail("僅邀請方可取消邀請。");

			row.StatusId = STATUS_REMOVED; // 專指：邀請被取消
			row.RequestedBy = null;          // 離開 PENDING，置空
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_REMOVED, "removed");
		}

		private async Task<RelationResult> DoBlock(Relation? row, int small, int large, int actor, CancellationToken ct)
		{
			if (row is null)
			{
				row = new Relation
				{
					UserIdSmall = small,
					UserIdLarge = large,
					StatusId = STATUS_BLOCKED,
					RequestedBy = null,          // 非 PENDING → 置空
					CreatedAt = DateTime.UtcNow
				};
				_db.Relations.Add(row);
				await _db.SaveChangesAsync(ct);
				return Ok(row.RelationId, STATUS_BLOCKED, "blocked");
			}

			if (row.StatusId == STATUS_BLOCKED)
				return NoOp(row.RelationId, STATUS_BLOCKED, "blocked");

			row.StatusId = STATUS_BLOCKED;
			row.RequestedBy = null;             // 非 PENDING → 置空
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_BLOCKED, "blocked");
		}

		private async Task<RelationResult> DoUnblock(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_BLOCKED) return Fail("不是封鎖狀態。");

			row.StatusId = STATUS_NONE;  // 解除封鎖 → 無關係
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_NONE, "none");
		}

		private async Task<RelationResult> DoUnfriend(Relation? row, int actor, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_ACCEPTED) return Fail("僅在好友狀態可解除好友。");

			row.StatusId = STATUS_NONE;  // 解除好友 → 無關係
			row.RequestedBy = null;
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_NONE, "none");
		}

		private async Task<RelationResult> DoSetNickname(Relation? row, string? nickname, CancellationToken ct)
		{
			if (row is null) return Fail("尚無此關係。");
			if (row.StatusId != STATUS_ACCEPTED) return Fail("非好友狀態不可設定暱稱。");

			var newName = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();
			if (newName is not null && newName.Length > NICKNAME_MAXLEN)
				return Fail($"暱稱長度上限 {NICKNAME_MAXLEN}。");

			if (string.Equals(row.FriendNickname, newName, StringComparison.Ordinal))
				return NoOp(row.RelationId, STATUS_ACCEPTED, "accepted");

			row.FriendNickname = newName;
			row.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);
			return Ok(row.RelationId, STATUS_ACCEPTED, "accepted");
		}

		// ============================================================================
		// 統一回傳
		// ============================================================================
		private static RelationResult Ok(int relationId, int newStatusId, string newCode)
			=> new(true, false, relationId, newCode, newStatusId, null);

		private static RelationResult NoOp(int relationId, int newStatusId, string newCode)
			=> new(true, true, relationId, newCode, newStatusId, null);

		private static RelationResult Fail(string reason)
			=> new(false, false, null, null, null, reason);
	}
}
