using Money.Abstractions;
using System.Linq;

namespace System.Collections.Generic;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns the first <see cref="ICurrency"/> provided by the <paramref name="providers"/> that matches the
    /// condition <paramref name="predicate"/>, or <see langword="null"/> if none exists.
    /// </summary>
    public static ICurrency GetFirstCurrency(this IEnumerable<ICurrencyProvider> providers, Func<ICurrency, bool> predicate) =>
        providers.SelectMany(currencyProvider => currencyProvider.Currencies).FirstOrDefault(predicate);
}
