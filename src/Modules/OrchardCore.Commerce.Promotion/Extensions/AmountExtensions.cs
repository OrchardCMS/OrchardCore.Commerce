using System;

namespace OrchardCore.Commerce.MoneyDataType;

public static class AmountExtensions
{
    public static Amount WithDiscount(this Amount amount, decimal discountPercentage) =>
        new(Math.Round(amount.Value * (1 - (discountPercentage / 100)), 2), amount.Currency);

    public static Amount WithDiscount(this Amount amount, Amount discountAmount)
    {
        if (!amount.Currency.Equals(discountAmount.Currency))
        {
            throw new InvalidOperationException($"The product's and discount's currencies are not the same: " +
                $"{amount.Currency.EnglishName}, {discountAmount.Currency.EnglishName}");
        }

        return new(Math.Round(Math.Max(0, amount.Value - discountAmount.Value), 2), amount.Currency);
    }
}
