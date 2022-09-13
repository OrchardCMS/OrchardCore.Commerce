using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class TaxShoppingCartEvents : IShoppingCartEvents
{
    private readonly IHtmlLocalizer<TaxShoppingCartEvents> H;
    private readonly IEnumerable<ITaxProvider> _taxProviders;
    public TaxShoppingCartEvents(
        IHtmlLocalizer<TaxShoppingCartEvents> htmlLocalizer,
        IEnumerable<ITaxProvider> taxProviders)
    {
        H = htmlLocalizer;
        _taxProviders = taxProviders;
    }

    public async Task<(IList<Amount> Totals, IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        IList<Amount> totals,
        IList<LocalizedHtmlString> headers,
        IList<ShoppingCartLineViewModel> lines)
    {
        var context = new TaxProviderContext(lines, totals);

        if (await _taxProviders.GetFirstApplicableProviderAsync(context) is not { } provider)
        {
            return (totals, headers, lines);
        }

        // Update lines and get new totals.
        context = await provider.UpdateAsync(context);
        foreach (var (price, index) in context.Items.Select((item, index) => (item.UnitPrice, index)))
        {
            lines[index].AdditionalData.SetGrossPrice(price);
        }

        var newHeaders = headers
            .Select(header => header.Name == "Price" ? H["Gross Price"] : header)
            .ToList();

        return (context.TotalsByCurrency.ToList(), newHeaders, lines);
    }
}
