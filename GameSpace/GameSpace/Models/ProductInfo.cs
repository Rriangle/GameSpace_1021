using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ProductInfo
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductType { get; set; } = null!;

    public decimal Price { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public int? ShipmentQuantity { get; set; }

    public int ProductCreatedBy { get; set; }

    public DateTime ProductCreatedAt { get; set; }

    public int? ProductUpdatedBy { get; set; }

    public DateTime? ProductUpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual GameProductDetail? GameProductDetail { get; set; }

    public virtual OtherProductDetail? OtherProductDetail { get; set; }

    public virtual ProductCode? ProductCode { get; set; }

    public virtual ManagerDatum ProductCreatedByNavigation { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductInfoAuditLog> ProductInfoAuditLogs { get; set; } = new List<ProductInfoAuditLog>();

    public virtual ManagerDatum? ProductUpdatedByNavigation { get; set; }
}
