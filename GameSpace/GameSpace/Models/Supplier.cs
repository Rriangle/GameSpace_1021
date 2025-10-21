using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<GameProductDetail> GameProductDetails { get; set; } = new List<GameProductDetail>();

    public virtual ICollection<OtherProductDetail> OtherProductDetails { get; set; } = new List<OtherProductDetail>();
}
