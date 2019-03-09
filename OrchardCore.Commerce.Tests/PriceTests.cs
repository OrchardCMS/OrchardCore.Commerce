using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class PriceTests
    {
        [Fact]
        public void PriceProviderAddsPriceFromPricePart()
        {
            var cart = new List<ShoppingCartItem>
            {
                new ShoppingCartItem (1, BuildProduct(50.0M)),
                new ShoppingCartItem (4, BuildProduct(30.0M)),
                new ShoppingCartItem (1, BuildProduct(10.0M))
            };
            var priceProvider = new PriceProvider();
            priceProvider.AddPrices(cart);

            foreach (var item in cart)
            {
                Assert.Single(item.Prices);
                Assert.Equal(item.Prices.Single().Price.Value, item.Product.As<PricePart>().Price.Value, 2);
            }
        }

        private static ContentItem BuildProduct(decimal price)
        {
            var product = new ContentItem();
            product.GetOrCreate<PricePart>();
            product.Alter<PricePart>(p => p.Price = new Amount(price, Currency.Dollar));
            return product;
        }
    }
}
