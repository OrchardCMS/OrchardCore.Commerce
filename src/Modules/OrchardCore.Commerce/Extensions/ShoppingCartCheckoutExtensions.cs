using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Extensions;
public static class ShoppingCartCheckoutExtensions
{
    public static async Task<IEnumerable<Amount>> CalculateTotalsAsync(
        this ShoppingCart shoppingCart,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy)
    {
        var lines = await shoppingCart.CalculateLinePricesAsync(priceService, priceSelectionStrategy);

        return lines
             .GroupBy(linePrice => linePrice.Currency)
             .Select(group => new Amount(group.Sum(linePrice => linePrice.Value), group.Key));
    }

    public static async Task<IEnumerable<Amount>> CalculateLinePricesAsync(
        this ShoppingCart shoppingCart,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy)
    {
        var items = await priceService.AddPricesAsync(shoppingCart.Items);
        return await Task.WhenAll(items.Select(item =>
        {
            var price = priceSelectionStrategy.SelectPrice(item.Prices);

            return Task.FromResult(item.Quantity * price);
        }));
    }
}
