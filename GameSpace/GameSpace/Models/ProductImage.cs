using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ProductImage
{
    public int ProductimgId { get; set; }

    public int ProductId { get; set; }

    public string ProductimgUrl { get; set; } = null!;

    public string? ProductimgAltText { get; set; }

    public DateTime ProductimgUpdatedAt { get; set; }

    public virtual ProductInfo Product { get; set; } = null!;
}
