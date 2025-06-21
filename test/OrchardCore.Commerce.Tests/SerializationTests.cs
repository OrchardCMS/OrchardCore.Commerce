using Moq.AutoMock;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ProductAttributeValues;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class SerializationTests
{
    [Fact]
    public async Task ShoppingCartSerializesAndDeserializes()
    {
        var cart = new ShoppingCart(
            new ShoppingCartItem(2, "product-1", prices:
            [
                new PrioritizedPrice(0, new Amount(10, Currency.Euro)),
                new PrioritizedPrice(1, new Amount(7, Currency.UsDollar)),
            ]),
            new ShoppingCartItem(
                1,
                "product-2",
                [
                    new BooleanProductAttributeValue("ProductPart3.attr1", value: true),
                    new NumericProductAttributeValue("ProductPart3.attr3", 42.0M),
                ],
                [new PrioritizedPrice(0, new Amount(12, Currency.UsDollar))]));

        var serializer = new AutoMocker().CreateShoppingCartSerializerInstance();
        var serialized = await serializer.SerializeAsync(cart);
        var result = await serializer.DeserializeAndVerifyAsync(serialized);
        var deserialized = result.ShoppingCart;

        result.HasChanged.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();

        deserialized.Count.ShouldBe(cart.Count);
        deserialized.ItemCount.ShouldBe(cart.ItemCount);

        deserialized.Items.ShouldBe(cart.Items);
    }
}
