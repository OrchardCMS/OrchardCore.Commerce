using GraphQL;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class PriceTests
{
    [Fact]
    public async Task PriceProviderAddsPriceFromPricePart()
    {
        var cart = new ShoppingCart(
            new ShoppingCartItem(1, "foo"),
            new ShoppingCartItem(4, "bar"),
            new ShoppingCartItem(1, "baz"));
        var productService = new DummyProductService(
            BuildProduct("foo", 50.0M),
            BuildProduct("bar", 30.0M),
            BuildProduct("baz", 10.0M));
        var priceProvider = new PriceProvider(productService, new TestMoneyService());
        cart = cart.With(await priceProvider.UpdateAsync(cart.Items));

        foreach (var item in cart.Items)
        {
            Assert.Single(item.Prices);
            Assert.Equal(
                item.Prices.Single().Price.Value,
                (await productService.GetProductAsync(item.ProductSku)).ContentItem.As<PricePart>().PriceField.Amount.Value,
                precision: 2);
        }
    }

    [Fact]
    public async Task PriceServiceAddsPricesInOrder()
    {
        var priceService = new PriceService(
            new List<IPriceProvider>
            {
                new DummyPriceProvider(4, 4.0m, isApplicable: false),
                new DummyPriceProvider(2, 2.0m, isApplicable: true),
                new DummyPriceProvider(1, 1.0m, isApplicable: false),
                new DummyPriceProvider(3, 3.0m, isApplicable: true),
            },
            priceSelectionStrategy: null);

        var cart = new ShoppingCart(new ShoppingCartItem(1, "foo"));
        cart = cart.With(await priceService.AddPricesAsync(cart.Items));

        Assert.Collection(
            cart.Items.Single().Prices,
            price => Assert.Equal(2.0m, price.Price.Value),
            price => Assert.Equal(3.0m, price.Price.Value));
    }

    [Fact]
    public void SimplePriceStrategySelectsLowestPriceForHighestStrategy()
    {
        var strategy = new SimplePriceStrategy();
        var selected = strategy.SelectPrice(new List<PrioritizedPrice>
        {
            new(0, new Amount(10, Currency.UsDollar)),
            new(1, new Amount(12, Currency.UsDollar)),
            new(1, new Amount(11, Currency.UsDollar)),
        });

        Assert.Equal(11, selected.Value);
    }

    private static ProductPart BuildProduct(string sku, decimal price)
    {
        var product = new ContentItem();
        product.GetOrCreate<PricePart>();
        product.Alter<PricePart>(pricePart => pricePart.Price = new Amount(price, Currency.Euro));
        product.GetOrCreate<ProductPart>();
        product.Alter<ProductPart>(productPart => productPart.Sku = sku);
        return product.As<ProductPart>();
    }

    private class DummyProductService : IProductService
    {
        private readonly Dictionary<string, ProductPart> _products;

        public DummyProductService(params ProductPart[] products) =>
            _products = products.ToDictionary(productPart => productPart.Sku);

        public Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus) =>
            Task.FromResult(skus.Select(sku => _products[sku]));

        // IProductService's method needs to be created, but implementation is unnecessary as the tests do not use it.
        public Task<(PriceVariantsPart Part, string VariantKey)> GetExactVariantAsync(string sku) => throw new NotSupportedException();

        // IProductService's method needs to be created, but implementation is unnecessary as the tests do not use it.
        public string GetVariantKey(string sku) => throw new NotSupportedException();
    }

    private class DummyPriceProvider : IPriceProvider
    {
        private readonly bool _isApplicable;
        public int Order { get; }

        public decimal Price { get; }

        public DummyPriceProvider(int priority, decimal price, bool isApplicable)
        {
            _isApplicable = isApplicable;
            Order = priority;
            Price = price;
        }

        public Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model) =>
            Task.FromResult<IList<ShoppingCartItem>>(model
                .Select(AddPriceToShoppingCartItem)
                .ToList());

        public Task<bool> IsApplicableAsync(IList<ShoppingCartItem> items) =>
             Task.FromResult(_isApplicable);

        private ShoppingCartItem AddPriceToShoppingCartItem(ShoppingCartItem item) =>
             item.WithPrice(
                    new PrioritizedPrice(0, new Amount(Price, Currency.UsDollar)));
    }
}
