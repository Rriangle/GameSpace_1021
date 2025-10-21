using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class OrderItem
{
    public int ItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int LineNo { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual OrderInfo Order { get; set; } = null!;
}
