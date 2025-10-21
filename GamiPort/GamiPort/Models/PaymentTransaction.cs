using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class PaymentTransaction
{
    public long PaymentId { get; set; }

    public long PaymentCode { get; set; }

    public int OrderId { get; set; }

    public string TxnType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Provider { get; set; } = null!;

    public string? ProviderTxn { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public string? Meta { get; set; }
}
