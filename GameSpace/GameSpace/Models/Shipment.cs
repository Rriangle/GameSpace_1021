using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Shipment
{
    public long ShipmentId { get; set; }

    public long ShipmentCode { get; set; }

    public int OrderId { get; set; }

    public string Carrier { get; set; } = null!;

    public string? TrackingNo { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }
}
