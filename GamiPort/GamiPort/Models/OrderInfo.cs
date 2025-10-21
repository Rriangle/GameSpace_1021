using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class OrderInfo
{
    public int OrderId { get; set; }

    public long OrderCode { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public decimal OrderTotal { get; set; }

    public DateTime? PaymentAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
