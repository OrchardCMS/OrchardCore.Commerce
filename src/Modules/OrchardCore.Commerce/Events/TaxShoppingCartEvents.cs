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

    public int Order => 0;

    public TaxShoppingCartEvents(
        IHtmlLocalizer<TaxShoppingCartEvents> htmlLocalizer,
        IEnumerable<ITaxProvider> taxProviders)
    {
        H = htmlLocalizer;
        _taxProviders = taxProviders;
    }

    public async Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        IList<Amount> totals,
        IList<LocalizedHtmlString> headers,
        IList<ShoppingCartLineViewModel> lines)
    {
        var context = new PromotionAndTaxProviderContext(lines, totals);

        if (await _taxProviders.GetFirstApplicableProviderAsync(context) is not { } provider)
        {
            return (headers, lines);
        }

        // Update lines and get new totals.
        context = await provider.UpdateAsync(context);
        foreach (var (price, index) in context.Items.Select((item, index) => (item.UnitPrice, index)))
        {
            lines[index].AdditionalData.SetGrossPrice(price);

            // Other promotions will use UnitPrice and LinePrice as the base of the promotion. We need to modify these
            // to the gross price, otherwise the promotion would be applied on the net price and that would be used.
            lines[index].LinePrice = price * lines[index].Quantity;
            lines[index].UnitPrice = price;
        }

        var newHeaders = headers
            .Select(header => header.Name == "Price" ? H["Gross Price"] : header)
            .ToList();

        return (newHeaders, lines);
    }
}
