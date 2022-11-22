using System;

namespace OrchardCore.Commerce.MoneyDataType;

public static class AmountExtensions
{
    public static Amount WithDiscount(this Amount amount, decimal discountPercentage) =>
        new(Math.Round(amount.Value * (1 - (discountPercentage / 100)), 2), amount.Currency);

    public static Amount WithDiscount(this Amount amount, Amount discountAmount) =>
        new(Math.Round(Math.Max(0, amount.Value - discountAmount.Value), 2), amount.Currency);

    public static bool IsValidAndNotZero(this Amount? amount) =>
        amount is { } notNullAmount && notNullAmount.IsValid && notNullAmount.Value != 0;

    public static bool IsValidAndNotZero(this Amount amount) =>
        amount.IsValid && amount.Value != 0;
}
