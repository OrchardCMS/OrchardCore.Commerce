using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.MoneyDataType.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns the first <see cref="ICurrency"/> provided by the <paramref name="providers"/> that matches the
    /// condition <paramref name="predicate"/>, or <see langword="null"/> if none exists.
    /// </summary>
    public static ICurrency GetFirstCurrency(this IEnumerable<ICurrencyProvider> providers, Func<ICurrency, bool> predicate) =>
        providers.SelectMany(currencyProvider => currencyProvider.Currencies).FirstOrDefault(predicate);

    public static Amount Sum(this IEnumerable<Amount> amounts)
    {
        var sum = 0m;
        ICurrency currency = null;

        foreach (var amount in amounts)
        {
            currency ??= amount.Currency;

            if (currency.CurrencyIsoCode != amount.Currency.CurrencyIsoCode)
            {
                throw new InvalidOperationException($"Each {nameof(Amount)} must be of the same currency.");
            }

            sum += amount.Value;
        }

        return currency == null ? Amount.Unspecified : new Amount(sum, currency);
    }

    public static IEnumerable<Amount> Round(this IEnumerable<Amount> amounts) =>
        amounts.Select(amount => amount.GetRounded());
}
