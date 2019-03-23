using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Abstractions;
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

        [Fact]
        public void PriceServiceAddsPricesInOrder()
        {
            var priceService = new PriceService(new List<IPriceProvider>
            {
                new DummyPriceProvider(4, 4.0m),
                new DummyPriceProvider(2, 2.0m),
                new DummyPriceProvider(1, 1.0m),
                new DummyPriceProvider(3, 3.0m)
            });
            var item = new ShoppingCartItem(1, BuildProduct(50.0M));
            var cart = new List<ShoppingCartItem> { item };
            priceService.AddPrices(cart);
            Assert.Collection(item.Prices,
                p => Assert.Equal(1.0m, p.Price.Value),
                p => Assert.Equal(2.0m, p.Price.Value),
                p => Assert.Equal(3.0m, p.Price.Value),
                p => Assert.Equal(4.0m, p.Price.Value));
        }

        private static ContentItem BuildProduct(decimal price)
        {
            var product = new ContentItem();
            product.GetOrCreate<PricePart>();
            product.Alter<PricePart>(p => p.Price = new Amount(price, Currency.Dollar));
            return product;
        }

        private class DummyPriceProvider : IPriceProvider
        {
            public DummyPriceProvider(int priority, decimal price)
            {
                Order = priority;
                Price = price;
            }

            public int Order { get; }
            public decimal Price { get; }

            public void AddPrices(IList<ShoppingCartItem> items)
            {
                foreach (var item in items)
                {
                    item.Prices.Add(new ProductPrice(new Amount(Price, Currency.Dollar)));
                }
            }
        }
    }
}
