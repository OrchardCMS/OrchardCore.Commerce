using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    public class PriceProvider : IPriceProvider
    {
        public int Order => 0;

        public void AddPrices(IList<ShoppingCartItem> items)
        {
            foreach (var item in items)
            {
                var pricePart = item.Product.As<PricePart>();
                if (pricePart != null) {
                    var price = pricePart.Price;
                    item.Prices.Add(new ProductPrice(price));
                }
            }
        }
    }
}
