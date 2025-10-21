using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class OfficialStoreRanking
{
    public int RankingId { get; set; }

    public string PeriodType { get; set; } = null!;

    public DateOnly RankingDate { get; set; }

    public int ProductId { get; set; }

    public string? RankingMetric { get; set; }

    public byte? RankingPosition { get; set; }

    public decimal? TradingAmount { get; set; }

    public int? TradingVolume { get; set; }

    public DateTime RankingUpdatedAt { get; set; }
}
