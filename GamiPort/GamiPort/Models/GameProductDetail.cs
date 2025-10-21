using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class GameProductDetail
{
    public int ProductId { get; set; }

    public string? ProductDescription { get; set; }

    public int SupplierId { get; set; }

    public int? PlatformId { get; set; }

    public string? PlatformName { get; set; }

    public string? GameType { get; set; }

    public string? DownloadLink { get; set; }

    public bool IsActive { get; set; }

    public virtual ProductInfo Product { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
