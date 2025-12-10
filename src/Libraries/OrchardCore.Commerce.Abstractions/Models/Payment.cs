using OrchardCore.Commerce.MoneyDataType;
using System;

namespace OrchardCore.Commerce.Abstractions.Models;

public record Payment(
    string Kind,
    string TransactionId,
    string ChargeText,
    Amount Amount,
    DateTime CreatedUtc);
