using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class SerializationTests
{
    [Fact]
    public async Task ShoppingCartSerializesAndDeserializes()
    {
        var cart = new ShoppingCart(
            new ShoppingCartItem(2, "product-1", prices: new[]
            {
                new PrioritizedPrice(0, new Amount(10, Currency.Euro)),
                new PrioritizedPrice(1, new Amount(7, Currency.UsDollar)),
            }),
            new ShoppingCartItem(
                1,
                "product-2",
                new IProductAttributeValue[]
                {
                    new BooleanProductAttributeValue("ProductPart3.attr1", value: true),
                    new NumericProductAttributeValue("ProductPart3.attr3", 42.0M),
                },
                new[] { new PrioritizedPrice(0, new Amount(12, Currency.UsDollar)) }));
        var serializer = new ShoppingCartSerializer(
            attributeProviders: new[] { new ProductAttributeProvider() },
            productService: new FakeProductService(),
            moneyService: new TestMoneyService(),
            contentDefinitionManager: new FakeContentDefinitionManager());
        var serialized = await serializer.SerializeAsync(cart);
        var deserialized = await serializer.DeserializeAsync(serialized);

        Assert.Equal(cart.Count, deserialized.Count);
        Assert.Equal(cart.ItemCount, deserialized.ItemCount);

        Assert.Equal(cart.Items, deserialized.Items);
    }
}
