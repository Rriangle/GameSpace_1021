using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class Evoucher
{
    public int EvoucherId { get; set; }

    public string EvoucherCode { get; set; } = null!;

    public int EvoucherTypeId { get; set; }

    public int UserId { get; set; }

    public bool IsUsed { get; set; }

    public DateTime AcquiredTime { get; set; }

    public DateTime? UsedTime { get; set; }

    public virtual ICollection<EvoucherRedeemLog> EvoucherRedeemLogs { get; set; } = new List<EvoucherRedeemLog>();

    public virtual ICollection<EvoucherToken> EvoucherTokens { get; set; } = new List<EvoucherToken>();

    public virtual EvoucherType EvoucherType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
