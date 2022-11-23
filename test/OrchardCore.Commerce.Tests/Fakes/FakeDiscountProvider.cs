using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeDiscountProvider : IPromotionProvider
{
    public int Order => 0;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model)
    {
        var items = model.Items.AsList();

        var newContextLineItems =
            items.Select(item => item with { UnitPrice = ApplyPromotionToShoppingCartItem(item) });

        var updatedTotals = model
            .TotalsByCurrency
            .Select(total =>
            {
                var currency = total.Currency.CurrencyIsoCode;
                return newContextLineItems
                    .Where(item => item.Subtotal.Currency.CurrencyIsoCode == currency)
                    .Select(item => item.Subtotal)
                    .Sum();
            });

        return Task.FromResult(new PromotionAndTaxProviderContext(newContextLineItems, updatedTotals));
    }

    // IPromotionProvider's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        throw new NotSupportedException();

    private static Amount ApplyPromotionToShoppingCartItem(PromotionAndTaxProviderContextLineItem item) =>
        new(item.UnitPrice.Value / 2, item.UnitPrice.Currency);
}
