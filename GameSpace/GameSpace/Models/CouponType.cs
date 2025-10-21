using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class CouponType
{
    public int CouponTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal MinSpend { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public int PointsCost { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
}
