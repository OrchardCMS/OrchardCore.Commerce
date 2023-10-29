using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using System;

namespace OrchardCore.Commerce.Models;

public record Payment(
    string Kind,
    string TransactionId,
    string ChargeText,
    Amount Amount,
    DateTime CreatedUtc)
    : IPayment;
