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

public class PromotionShoppingCartEvents : IShoppingCartEvents
{
    private readonly IPromotionService _promotionService;

    // Promotions should be applied after taxes.
    public int Order => int.MaxValue;

    public PromotionShoppingCartEvents(IPromotionService promotionService) =>
        _promotionService = promotionService;

    public async Task<(IList<Amount> Totals, IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        IList<Amount> totals,
        IList<LocalizedHtmlString> headers,
        IList<ShoppingCartLineViewModel> lines)
    {
        var context = new PromotionAndTaxProviderContext(lines, totals);

        // Update lines and get new totals.
        context = await _promotionService.AddPromotionsAsync(context);

        if (headers.Any(header => header.Name == "Gross Price"))
        {
            foreach (var (price, index) in context.Items.Select((item, index) => (item.UnitPrice, index)))
            {
                lines[index].AdditionalData.SetGrossPrice(price);
            }
        }

        return (context.TotalsByCurrency.ToList(), headers, lines);
    }
}
