using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ProductCode
{
    public string ProductCode1 { get; set; } = null!;

    public int ProductId { get; set; }

    public virtual ProductInfo Product { get; set; } = null!;
}
