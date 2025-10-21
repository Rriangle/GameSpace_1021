using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class MerchType
{
    public int MerchTypeId { get; set; }

    public string MerchTypeName { get; set; } = null!;

    public virtual ICollection<OtherProductDetail> OtherProductDetails { get; set; } = new List<OtherProductDetail>();
}
