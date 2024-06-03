using System;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.MoneyDataType;

namespace OrchardCore.Commerce.Abstractions.Models;

public record Payment(
    string Kind,
    string TransactionId,
    string ChargeText,
    Amount Amount,
    DateTime CreatedUtc)
    : IPayment;
