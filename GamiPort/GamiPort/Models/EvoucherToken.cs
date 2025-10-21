using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class EvoucherToken
{
    public int TokenId { get; set; }

    public int EvoucherId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public virtual Evoucher Evoucher { get; set; } = null!;

    public virtual ICollection<EvoucherRedeemLog> EvoucherRedeemLogs { get; set; } = new List<EvoucherRedeemLog>();
}
