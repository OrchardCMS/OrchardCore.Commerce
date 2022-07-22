using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakePriceService : IPriceService
{
    public Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items) =>
        Task.FromResult<IList<ShoppingCartItem>>(
            items
                .Select((item, index) => AddPriceToShoppingCartItem(item, index))
                .ToList());

    public Task<ShoppingCartItem> AddPriceAsync(ShoppingCartItem item) =>
        Task.FromResult(AddPriceToShoppingCartItem(item));

    private static ShoppingCartItem AddPriceToShoppingCartItem(ShoppingCartItem item, int index = 0) =>
        item.WithPrice(new PrioritizedPrice(priority: 0, price: new Amount(42 + index, Currency.UsDollar)));
}
