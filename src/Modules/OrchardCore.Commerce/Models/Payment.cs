using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using System;

namespace OrchardCore.Commerce.Models;

public class Payment : IPayment
{
    public string Kind { get; set; }

    public string TransactionId { get; set; }

    public string ChargeText { get; set; }

    public Amount Amount { get; set; }

    public DateTime CreatedUtc { get; set; }
}
