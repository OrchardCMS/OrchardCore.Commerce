using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.Commerce.ViewModels;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class ShoppingCartControllerTests
    {
        private readonly IShoppingCartPersistence _cartStorage;
        private readonly ShoppingCartController _controller;

        private readonly Dictionary<string, string[]> _attrSet1 = new Dictionary<string, string[]>
        {
            { "ProductPart3.attr1", new[] { "true" } }
        };
        private Dictionary<string, string[]> _attrSet2 = new Dictionary<string, string[]>
        {
            {  "ProductPart3.attr1", new[] { "false" } }
        };
        private Dictionary<string, string[]> _attrSet3 = new Dictionary<string, string[]>
        {
            { "ProductPart3.attr1", new[] { "true" } },
            { "ProductPart3.attr2", new[] { "bar", "baz" } }
        };
        private readonly HashSet<IProductAttributeValue> _attrSet1Parsed = new HashSet<IProductAttributeValue>
        {
            new BooleanProductAttributeValue("ProductPart3.attr1", true)
        };
        private readonly HashSet<IProductAttributeValue> _attrSet2Parsed = new HashSet<IProductAttributeValue>
        {
            new BooleanProductAttributeValue("ProductPart3.attr1", false)
        };
        private readonly HashSet<IProductAttributeValue> _attrSet3Parsed = new HashSet<IProductAttributeValue>
        {
            new BooleanProductAttributeValue("ProductPart3.attr1", true),
            new TextProductAttributeValue("ProductPart3.attr2", "bar", "baz")
        };

        public ShoppingCartControllerTests() {
            _cartStorage = new FakeCartStorage();
            _controller = new ShoppingCartController(
                shoppingCartPersistence: _cartStorage,
                shoppingCartHelpers: new ShoppingCartHelpers(
                    attributeProviders: new[] { new ProductAttributeProvider() },
                    productService: new FakeProductService(),
                    moneyService: new TestMoneyService(),
                    contentDefinitionManager: new FakeContentDefinitionManager()
                ),
                productService: new FakeProductService(),
                priceService: new FakePriceService(),
                priceStrategy: new SimplePriceStrategy(),
                contentManager: new FakeContentManager() 
            );
        }

        [Fact]
        public async Task AddExistingItemToCart()
        {
            await _cartStorage.Store(new List<ShoppingCartItem> { new ShoppingCartItem(3, "foo") });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 7, ProductSku = "foo" });
            var cart = await _cartStorage.Retrieve();

            Assert.Equal(new List<ShoppingCartItem> { new ShoppingCartItem(10, "foo") }, cart);
        }

        [Fact]
        public async Task AddNewItemToCart()
        {
            await _cartStorage.Store(new List<ShoppingCartItem> { new ShoppingCartItem(3, "foo") });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 7, ProductSku = "bar" });
            var cart = await _cartStorage.Retrieve();

            Assert.Equal(new List<ShoppingCartItem>
            {
                new ShoppingCartItem(3, "foo"),
                new ShoppingCartItem(7, "bar")
            }, cart);
        }

        [Fact]
        public async Task AddExistingItemWithAttributes()
        {
            await _cartStorage.Store(new List<ShoppingCartItem>
            {
                new ShoppingCartItem(2, "foo"),
                new ShoppingCartItem(3, "foo", _attrSet1Parsed),
                new ShoppingCartItem(4, "foo", _attrSet2Parsed),
                new ShoppingCartItem(5, "foo", _attrSet3Parsed),
                new ShoppingCartItem(6, "bar", _attrSet3Parsed)
            });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 7, ProductSku = "foo" });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 8, ProductSku = "foo", Attributes = _attrSet1 });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 9, ProductSku = "foo", Attributes = _attrSet2 });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 10, ProductSku = "foo", Attributes = _attrSet3 });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 11, ProductSku = "bar", Attributes = _attrSet3 });
            await _controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 13, ProductSku = "baz", Attributes = _attrSet3 });
            var cart = await _controller.Get();

            Assert.Equal(new List<ShoppingCartItem>
            {
                new ShoppingCartItem(9, "foo"),
                new ShoppingCartItem(11, "foo", _attrSet1Parsed),
                new ShoppingCartItem(13, "foo", _attrSet2Parsed),
                new ShoppingCartItem(15, "foo", _attrSet3Parsed),
                new ShoppingCartItem(17, "bar", _attrSet3Parsed),
                new ShoppingCartItem(13, "baz", _attrSet3Parsed)
            }, cart);
        }

        [Fact]
        public async Task RemoveItems()
        {
            var expectedCart = new List<ShoppingCartItem>
            {
                new ShoppingCartItem(2, "foo"),
                new ShoppingCartItem(3, "foo", _attrSet1Parsed),
                new ShoppingCartItem(4, "foo", _attrSet2Parsed),
                new ShoppingCartItem(5, "foo", _attrSet3Parsed),
                new ShoppingCartItem(6, "bar", _attrSet3Parsed)
            };
            await _cartStorage.Store(expectedCart);

            await _controller.RemoveItem(new ShoppingCartLineUpdateModel { Quantity = 0, ProductSku = "foo", Attributes = _attrSet2 });
            expectedCart.RemoveAt(2); // foo - attr2
            Assert.Equal(expectedCart, await _controller.Get());

            // Removing an item that's no longer there does nothing
            await _controller.RemoveItem(new ShoppingCartLineUpdateModel { Quantity = 0, ProductSku = "foo", Attributes = _attrSet2 });
            Assert.Equal(expectedCart, await _controller.Get());

            await _controller.RemoveItem(new ShoppingCartLineUpdateModel { Quantity = 0, ProductSku = "bar", Attributes = _attrSet3 });
            expectedCart.RemoveAt(3); // bar - attr3
            Assert.Equal(expectedCart, await _controller.Get());

            await _controller.RemoveItem(new ShoppingCartLineUpdateModel { Quantity = 0, ProductSku = "foo" });
            expectedCart.RemoveAt(0); // foo
            Assert.Equal(expectedCart, await _controller.Get());

            await _controller.RemoveItem(new ShoppingCartLineUpdateModel { Quantity = 0, ProductSku = "foo", Attributes = _attrSet1 });
            expectedCart.RemoveAt(0); // foo - attr1
            Assert.Equal(expectedCart, await _controller.Get());

            await _controller.RemoveItem(new ShoppingCartLineUpdateModel { Quantity = 0, ProductSku = "foo", Attributes = _attrSet3 });
            expectedCart.RemoveAt(0); // foo - attr3
            Assert.Equal(expectedCart, await _controller.Get());
        }
    }
}
