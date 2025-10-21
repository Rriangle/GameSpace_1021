using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class WalletHistory
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string ChangeType { get; set; } = null!;

    public int PointsChanged { get; set; }

    public string? ItemCode { get; set; }

    public string? Description { get; set; }

    public DateTime ChangeTime { get; set; }

    public virtual User User { get; set; } = null!;
}
