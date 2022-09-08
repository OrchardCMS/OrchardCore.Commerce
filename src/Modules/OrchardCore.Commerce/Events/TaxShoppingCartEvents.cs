using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class TaxShoppingCartEvents : IShoppingCartEvents
{
    private readonly IHtmlLocalizer<TaxShoppingCartEvents> H;
    public TaxShoppingCartEvents(IHtmlLocalizer<TaxShoppingCartEvents> htmlLocalizer) =>
        H = htmlLocalizer;

    public Task LinesDisplayingAsync(IList<LocalizedHtmlString> headers, ShoppingCartLineViewModel[] lines)
    {
        if (IsApplicable(lines))
        {
            var priceHeaderIndex = headers.IndexOf(headers.Single(header => header.Name == "Price"));
            headers[priceHeaderIndex] = H["Gross Price"];
        }

        return Task.CompletedTask;
    }

    public Task TotalsDisplayingAsync(IList<Amount> totals, ShoppingCartLineViewModel[] lines)
    {
        if (IsApplicable(lines))
        {
            for (var i = 0; i < totals.Count; i++)
            {
                var currency = totals[i].Currency.CurrencyIsoCode;
                totals[i] = lines
                    .Where(line => line.LinePrice.Currency.CurrencyIsoCode == currency)
                    .Select(line => line.LinePrice.WithTax(line.Product))
                    .Sum();
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsApplicable(ShoppingCartLineViewModel[] lines)
    {
        var countWithGrossPrice = lines
            .Select(line => line.Product.ContentItem.As<TaxPart>())
            .Count(taxPart => taxPart?.GrossPrice?.Amount.IsValid == true && taxPart.TaxRate.Value > 0);

        if (countWithGrossPrice == 0) return false;

        if (countWithGrossPrice < lines.Length)
        {
            throw new InvalidOperationException("Some, but not all products have gross price. This is invalid.");
        }

        return true;
    }
}
