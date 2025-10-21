using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class OrderAddress
{
    public int OrderId { get; set; }

    public string Recipient { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Zipcode { get; set; } = null!;

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;
}
