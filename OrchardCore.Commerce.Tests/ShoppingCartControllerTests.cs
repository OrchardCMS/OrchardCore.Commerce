using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class ShoppingCartControllerTests
    {
        [Fact]
        public async Task AddExistingItemToCart()
        {
            var cartStorage = new FakeCartStorage(new List<ShoppingCartItem> { new ShoppingCartItem(3, "foo") });
            var controller = new ShoppingCartController(cartStorage);
            var cart = await controller.AddItem(new ShoppingCartItem(7, "foo"));

            Assert.Equal(new List<ShoppingCartItem> { new ShoppingCartItem(10, "foo") }, cart);
        }

        [Fact]
        public async Task AddNewItemToCart()
        {
            var cartStorage = new FakeCartStorage(new List<ShoppingCartItem> { new ShoppingCartItem(3, "foo") });
            var controller = new ShoppingCartController(cartStorage);
            var cart = await controller.AddItem(new ShoppingCartItem(7, "bar"));

            Assert.Equal(new List<ShoppingCartItem>
            {
                new ShoppingCartItem(3, "foo"),
                new ShoppingCartItem(7, "bar")
            }, cart);
        }

        [Fact]
        public async Task AddExistingItemWithAttributes()
        {
            var attrSet1 = new HashSet<IProductAttributeValue>
            {
                new BooleanProductAttributeValue("attr1", true)
            };
            var attrSet2 = new HashSet<IProductAttributeValue>
            {
                new BooleanProductAttributeValue("attr1", false)
            };
            var attrSet3 = new HashSet<IProductAttributeValue>
            {
                new BooleanProductAttributeValue("attr1", true),
                new TextProductAttributeValue("attr2", "bar", "baz")
            };
            var cartStorage = new FakeCartStorage(new List<ShoppingCartItem>
            {
                new ShoppingCartItem(2, "foo"),
                new ShoppingCartItem(3, "foo", attrSet1),
                new ShoppingCartItem(4, "foo", attrSet2),
                new ShoppingCartItem(5, "foo", attrSet3),
                new ShoppingCartItem(6, "bar", attrSet3)
            });
            var controller = new ShoppingCartController(cartStorage);
            await controller.AddItem(new ShoppingCartItem(7, "foo"));
            await controller.AddItem(new ShoppingCartItem(8, "foo", attrSet1));
            await controller.AddItem(new ShoppingCartItem(9, "foo", attrSet2));
            await controller.AddItem(new ShoppingCartItem(10, "foo", attrSet3));
            await controller.AddItem(new ShoppingCartItem(11, "bar", attrSet3));
            await controller.AddItem(new ShoppingCartItem(13, "baz", attrSet3));
            var cart = await controller.Index();

            Assert.Equal(new List<ShoppingCartItem>
            {
                new ShoppingCartItem(9, "foo"),
                new ShoppingCartItem(11, "foo", attrSet1),
                new ShoppingCartItem(13, "foo", attrSet2),
                new ShoppingCartItem(15, "foo", attrSet3),
                new ShoppingCartItem(17, "bar", attrSet3),
                new ShoppingCartItem(13, "baz", attrSet3)
            }, cart);
        }

        [Fact]
        public async Task RemoveItems()
        {
            var attrSet1 = new HashSet<IProductAttributeValue>
            {
                new BooleanProductAttributeValue("attr1", true)
            };
            var attrSet2 = new HashSet<IProductAttributeValue>
            {
                new BooleanProductAttributeValue("attr1", false)
            };
            var attrSet3 = new HashSet<IProductAttributeValue>
            {
                new BooleanProductAttributeValue("attr1", true),
                new TextProductAttributeValue("attr2", "bar", "baz")
            };
            var expectedCart = new List<ShoppingCartItem>
            {
                new ShoppingCartItem(2, "foo"),
                new ShoppingCartItem(3, "foo", attrSet1),
                new ShoppingCartItem(4, "foo", attrSet2),
                new ShoppingCartItem(5, "foo", attrSet3),
                new ShoppingCartItem(6, "bar", attrSet3)
            };
            var cartStorage = new FakeCartStorage(expectedCart);
            var controller = new ShoppingCartController(cartStorage);

            await controller.RemoveItem(new ShoppingCartItem(0, "foo", attrSet2));
            expectedCart.RemoveAt(2);
            Assert.Equal(expectedCart, await controller.Index());

            // Removing an item that's no longer there does nothing
            await controller.RemoveItem(new ShoppingCartItem(0, "foo", attrSet2));
            Assert.Equal(expectedCart, await controller.Index());

            await controller.RemoveItem(new ShoppingCartItem(0, "bar", attrSet3));
            expectedCart.RemoveAt(3);
            Assert.Equal(expectedCart, await controller.Index());

            await controller.RemoveItem(new ShoppingCartItem(0, "foo"));
            expectedCart.RemoveAt(0);
            Assert.Equal(expectedCart, await controller.Index());

            await controller.RemoveItem(new ShoppingCartItem(0, "foo", attrSet1));
            expectedCart.RemoveAt(0);
            Assert.Equal(expectedCart, await controller.Index());

            await controller.RemoveItem(new ShoppingCartItem(0, "foo", attrSet3));
            expectedCart.RemoveAt(0);
            Assert.Equal(expectedCart, await controller.Index());
        }

        private class FakeCartStorage : IShoppingCartPersistence
        {
            private Dictionary<string, IList<ShoppingCartItem>> _carts = new Dictionary<string, IList<ShoppingCartItem>>();

            public FakeCartStorage(IList<ShoppingCartItem> items = null, string cartId = null)
            {
                _carts[cartId ?? ""] = new List<ShoppingCartItem>(items) ?? new List<ShoppingCartItem>();
            }

            public Task<IList<ShoppingCartItem>> Retrieve(string shoppingCartId = null)
            {
                if (!_carts.TryGetValue(shoppingCartId ?? "", out var cart))
                {
                    cart = new List<ShoppingCartItem>();
                    _carts.Add(shoppingCartId ?? "", cart);
                }
                return Task.FromResult(cart);
            }

            public Task Store(IList<ShoppingCartItem> items, string shoppingCartId = null)
            {
                _carts[shoppingCartId ?? ""] = items;
                return Task.CompletedTask;
            }
        }
    }
}
