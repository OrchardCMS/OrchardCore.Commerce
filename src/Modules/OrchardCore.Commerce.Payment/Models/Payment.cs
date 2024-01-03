using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using System;

namespace OrchardCore.Commerce.Payment.Models;

public record Payment(
    string Kind,
    string TransactionId,
    string ChargeText,
    Amount Amount,
    DateTime CreatedUtc)
    : IPayment;
