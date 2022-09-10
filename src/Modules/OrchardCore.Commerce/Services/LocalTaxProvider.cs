using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
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
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    // This is the simplest, lowest priority option and if anything else is applicable it should supersede this.
    public int Order => int.MaxValue;

    public LocalTaxProvider(IShoppingCartHelpers shoppingCartHelpers) =>
        _shoppingCartHelpers = shoppingCartHelpers;

    public Task<TaxProviderContext> UpdateAsync(TaxProviderContext model)
    {
        var subtotals = model.Subtotals.AsList();
        var products = model.Contents.AsList();

        var updatedTotals = model
            .TotalsByCurrency
            .Select((total, index) =>
            {
                var currency = total.Currency.CurrencyIsoCode;
                return subtotals
                    .Where(amount => amount.Currency.CurrencyIsoCode == currency)
                    .Select(amount => amount.WithTax(products[index]))
                    .Sum();
            });

        return Task.FromResult(new TaxProviderContext(
            products,
            products.Select(content => content.ContentItem.As<TaxPart>().GrossPrice.Amount),
            updatedTotals));
    }

    public Task<bool> IsApplicableAsync(TaxProviderContext model) =>
        Task.FromResult(IsApplicable(model.Contents.Select(content => content.ContentItem.As<TaxPart>()).ToList()));

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
