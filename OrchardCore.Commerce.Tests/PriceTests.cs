using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async void PriceProviderAddsPriceFromPricePart()
        {
            var cart = new List<ShoppingCartItem>
            {
                new ShoppingCartItem (1, "foo"),
                new ShoppingCartItem (4, "bar"),
                new ShoppingCartItem (1, "baz")
            };
            var productService = new DummyProductService(
                BuildProduct("foo", 50.0M),
                BuildProduct("bar", 30.0M),
                BuildProduct("baz", 10.0M));
            var priceProvider = new PriceProvider(productService);
            await priceProvider.AddPrices(cart);

            foreach (var item in cart)
            {
                Assert.Single(item.Prices);
                Assert.Equal(item.Prices.Single().Value, (await productService.GetProduct(item.ProductSku)).ContentItem.As<PricePart>().Price.Value, 2);
            }
        }

        [Fact]
        public async Task PriceServiceAddsPricesInOrder()
        {
            var priceService = new PriceService(new List<IPriceProvider>
            {
                new DummyPriceProvider(4, 4.0m),
                new DummyPriceProvider(2, 2.0m),
                new DummyPriceProvider(1, 1.0m),
                new DummyPriceProvider(3, 3.0m)
            });
            var item = new ShoppingCartItem(1, "foo");
            var cart = new List<ShoppingCartItem> { item };
            await priceService.AddPrices(cart);
            Assert.Collection(item.Prices,
                p => Assert.Equal(1.0m, p.Value),
                p => Assert.Equal(2.0m, p.Value),
                p => Assert.Equal(3.0m, p.Value),
                p => Assert.Equal(4.0m, p.Value));
        }

        private static ProductPart BuildProduct(string sku, decimal price)
        {
            var product = new ContentItem();
            product.GetOrCreate<PricePart>();
            product.Alter<PricePart>(p => p.Price = new Amount(price, Currency.USDollar));
            product.GetOrCreate<ProductPart>();
            product.Alter<ProductPart>(p => p.Sku = sku);
            return product.As<ProductPart>();
        }

        private class DummyProductService : IProductService
        {
            private Dictionary<string, ProductPart> _products;

            public DummyProductService(params ProductPart[] products)
            {
                _products = products.ToDictionary(p => p.Sku);
            }

            public Task<ProductPart> GetProduct(string sku)
                => Task.FromResult(_products[sku]);

            public Task<IEnumerable<ProductPart>> GetProducts(IEnumerable<string> skus)
                => Task.FromResult(skus.Select(sku => _products[sku]));
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

            public Task AddPrices(IList<ShoppingCartItem> items)
            {
                foreach (var item in items)
                {
                    item.Prices.Add(new Amount(Price, Currency.USDollar));
                }
                return Task.CompletedTask;
            }
        }
    }
}
