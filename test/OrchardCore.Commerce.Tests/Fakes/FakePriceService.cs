using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

internal sealed class FakePriceService : IPriceService
{
    public Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items) =>
        Task.FromResult<IList<ShoppingCartItem>>(
            items.Select(AddPriceToShoppingCartItem).ToList());

    public Amount SelectPrice(IEnumerable<PrioritizedPrice> prices) => prices.First().Price;

    private static ShoppingCartItem AddPriceToShoppingCartItem(ShoppingCartItem item, int index = 0) =>
        item.WithPrice(new PrioritizedPrice(priority: 0, price: new Amount(42 + index, Currency.UsDollar)));
}
