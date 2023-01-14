using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Extensions;

public static class LineItemExtensions
{
    public static IEnumerable<Amount> CalculateTotals(this IEnumerable<ILineItem> lines) =>
        lines
            .GroupBy(line => line.UnitPrice.Currency.CurrencyIsoCode)
            .Select(group => group.Select(line => line.LinePrice).Sum())
            .Where(total => total.IsValidAndNonZero);
}
