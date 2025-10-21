using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class StockMovement
{
    public long MovementId { get; set; }

    public int ProductId { get; set; }

    public int? OrderId { get; set; }

    public int ChangeQty { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }
}
