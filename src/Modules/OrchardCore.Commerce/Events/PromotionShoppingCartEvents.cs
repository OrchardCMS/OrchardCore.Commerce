using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class PromotionShoppingCartEvents : ShoppingCartEventsBase
{
    private readonly IClock _clock;
    private readonly IHtmlLocalizer<PromotionShoppingCartEvents> H;
    private readonly IPromotionService _promotionService;

    // Promotions should be applied after taxes.
    public override int Order => int.MaxValue;

    public PromotionShoppingCartEvents(
        IClock clock,
        IHtmlLocalizer<PromotionShoppingCartEvents> htmlLocalizer,
        IPromotionService promotionService)
    {
        _clock = clock;
        H = htmlLocalizer;
        _promotionService = promotionService;
    }

    public override async Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        ShoppingCartDisplayingEventContext eventContext)
    {
        var headers = eventContext.Headers;
        var lines = eventContext.Lines;
        var context = new PromotionAndTaxProviderContext(
            lines,
            eventContext.Totals,
            eventContext.ShippingAddress,
            eventContext.BillingAddress,
            _clock.UtcNow);

        if (!await _promotionService.IsThereAnyApplicableProviderAsync(context))
        {
            return (headers, lines);
        }

        var newHeaders = headers.ToList();

        var insertIndex = newHeaders.FindIndex(header => header.Name is "Price" or "Gross Price");
        newHeaders.Insert(insertIndex, H["Old Price"]);

        foreach (var (price, index) in lines.Select((item, index) => (item.UnitPrice, index)))
        {
            lines[index].AdditionalData.SetOldPrice(price);
        }

        // Update lines and get new totals.
        context = await _promotionService.AddPromotionsAsync(context);

        foreach (var (item, index) in context.Items.Select((item, index) => (item, index)))
        {
            var line = lines[index];
            var price = item.UnitPrice;

            line.AdditionalData.SetDiscounts(item.Discounts);

            if (line.AdditionalData.HasGrossPrice())
            {
                var ratio = line.AdditionalData.GetNetPrice().Value / line.AdditionalData.GetGrossPrice().Value;

                line.AdditionalData.SetGrossPrice(price);
                line.AdditionalData.SetNetPrice(price * ratio);
            }

            line.LinePrice = price * lines[index].Quantity;
            line.UnitPrice = price;
        }

        return (newHeaders, lines);
    }
}
