using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class LocalTaxProvider : ITaxProvider
{
    // This is the simplest, lowest priority option and if anything else is applicable it should supersede this.
    public int Order => int.MaxValue;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model)
    {
        var items = model.Items.AsList();

        var updatedTotals = model
            .TotalsByCurrency
            .Select(total =>
            {
                var currency = total.Currency.CurrencyIsoCode;
                return items
                    .Where(item => item.Subtotal.Currency.CurrencyIsoCode == currency)
                    .Select(item => item.Subtotal.WithTax(item.Content))
                    .Sum();
            });

        return Task.FromResult(new PromotionAndTaxProviderContext(
            items.Select(item => item with { UnitPrice = item.UnitPrice.WithTax(item.Content) }),
            updatedTotals));
    }

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        Task.FromResult(IsApplicable(model.Items.Select(item => item.Content.ContentItem.As<TaxPart>()).ToList()));

    private static bool IsApplicable(IList<TaxPart> taxParts)
    {
        var countWithGrossPrice = taxParts
            .Count(taxPart => taxPart?.GrossPrice?.Amount.IsValid == true && taxPart.TaxRate.Value > 0);

        if (countWithGrossPrice == 0) return false;

        if (countWithGrossPrice < taxParts.Count)
        {
            throw new InvalidOperationException("Some, but not all products have gross price. This is invalid.");
        }

        return true;
    }
}
