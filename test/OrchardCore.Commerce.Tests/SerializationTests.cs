using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class SerializationTests
    {
        [Fact]
        public async Task ShoppingCartSerializesAndDeserializes()
        {
            var cart = new ShoppingCart(
                new ShoppingCartItem(2, "product-1", prices: new[]
                {
                    new PrioritizedPrice(0, new Amount(10, Currency.Euro)),
                    new PrioritizedPrice(1, new Amount(7, Currency.USDollar))
                }),
                new ShoppingCartItem(1, "product-2", attributes: new IProductAttributeValue[]
                {
                    new BooleanProductAttributeValue("ProductPart3.attr1", true),
                    new NumericProductAttributeValue("ProductPart3.attr3", (decimal?)42.0)
                }, prices: new[]
                {
                    new PrioritizedPrice(0, new Amount(12, Currency.USDollar))
                }));
            var helpers = new ShoppingCartHelpers(
                attributeProviders: new[] { new ProductAttributeProvider() },
                productService: new FakeProductService(),
                moneyService: new TestMoneyService(),
                contentDefinitionManager: new FakeContentDefinitionManager());
            string serialized = await helpers.Serialize(cart);
            ShoppingCart deserialized = await helpers.Deserialize(serialized);

            Assert.Equal(cart.Count, deserialized.Count);
            Assert.Equal(cart.ItemCount, deserialized.ItemCount);

            Assert.Equal(cart.Items, deserialized.Items);
        }
    }
}
