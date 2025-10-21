using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class OtherProductDetail
{
    public int ProductId { get; set; }

    public string? ProductDescription { get; set; }

    public int SupplierId { get; set; }

    public int? MerchTypeId { get; set; }

    public string? DigitalCode { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? Weight { get; set; }

    public string? Dimensions { get; set; }

    public string? Material { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public virtual MerchType? MerchType { get; set; }

    public virtual ProductInfo Product { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
