using Lombiq.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq.AutoMock;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Endpoints;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class ShoppingCartControllerTests
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    // This change would break the IShoppingCartPersistence reference in the consumer classes and
    // it would lead to multiple test failing due to NRE when accessing methods in IShoppingCartPersistence.
    private readonly IShoppingCartPersistence _cartStorage = new FakeCartStorage();
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    private readonly Dictionary<string, string[]> _attrSet1 = new()
    {
        { "ProductPart3.attr1", new[] { "true" } },
    };
    private readonly Dictionary<string, string[]> _attrSet2 = new()
    {
        { "ProductPart3.attr1", new[] { "false" } },
    };
    private readonly Dictionary<string, string[]> _attrSet3 = new()
    {
        { "ProductPart3.attr1", new[] { "true" } },
        { "ProductPart3.attr2", new[] { "bar", "baz" } },
    };
    private readonly HashSet<IProductAttributeValue> _attrSet1Parsed =
    [
        new BooleanProductAttributeValue("ProductPart3.attr1", value: true),
    ];
    private readonly HashSet<IProductAttributeValue> _attrSet2Parsed =
    [
        new BooleanProductAttributeValue("ProductPart3.attr1", value: false),
    ];
    private readonly HashSet<IProductAttributeValue> _attrSet3Parsed =
    [
        new BooleanProductAttributeValue("ProductPart3.attr1", value: true),
        new TextProductAttributeValue("ProductPart3.attr2", "bar", "baz"),
    ];

    [Fact]
    public async Task AddExistingItemToCart()
    {
        var cart = await StoreAndRetrieveItemAsync("foo");

        Assert.Equal(
            [new(10, "foo")],
            cart.Items);
    }

    [Fact]
    public async Task AddNewItemToCart()
    {
        var cart = await StoreAndRetrieveItemAsync("bar");

        Assert.Equal(
            [
                new(3, "foo"),
                new(7, "bar"),
            ],
            cart.Items);
    }

    [Fact]
    public async Task AddExistingItemWithAttributes()
    {
        await _cartStorage.StoreAsync(new ShoppingCart(
            new ShoppingCartItem(2, "foo"),
            new ShoppingCartItem(3, "foo", _attrSet1Parsed),
            new ShoppingCartItem(4, "foo", _attrSet2Parsed),
            new ShoppingCartItem(5, "foo", _attrSet3Parsed),
            new ShoppingCartItem(6, "bar", _attrSet3Parsed)));

        using var controller = GetController();
        await AddItemAsync(controller, cartId: null, 7, "foo");
        await AddItemAsync(controller, cartId: null, 8, "foo", _attrSet1);
        await AddItemAsync(controller, cartId: null, 9, "foo", _attrSet2);
        await AddItemAsync(controller, cartId: null, 10, "foo", _attrSet3);
        await AddItemAsync(controller, cartId: null, 11, "bar", _attrSet3);
        await AddItemAsync(controller, cartId: null, 13, "baz", _attrSet3);

        var cart = await controller.Get();

        Assert.Equal(
            [
                new(9, "foo"),
                new(11, "foo", _attrSet1Parsed),
                new(13, "foo", _attrSet2Parsed),
                new(15, "foo", _attrSet3Parsed),
                new(17, "bar", _attrSet3Parsed),
                new(13, "baz", _attrSet3Parsed),
            ],
            cart.Items);
    }

    [Fact]
    public async Task RemoveItems()
    {
        var expectedCartItems = new List<ShoppingCartItem>
        {
            new(2, "foo"),
            new(3, "foo", _attrSet1Parsed),
            new(4, "foo", _attrSet2Parsed),
            new(5, "foo", _attrSet3Parsed),
            new(6, "bar", _attrSet3Parsed),
        };
        var cart = new ShoppingCart(expectedCartItems);
        await _cartStorage.StoreAsync(cart);

        using var controller = GetController();

        await controller.RemoveItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 0,
            ProductSku = "foo",
            Attributes = _attrSet2,
        });
        expectedCartItems.RemoveAt(2); // foo - attr2
        Assert.Equal(expectedCartItems, (await controller.Get()).Items);

        // Removing an item that's no longer there does nothing.
        await controller.RemoveItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 0,
            ProductSku = "foo",
            Attributes = _attrSet2,
        });
        Assert.Equal(expectedCartItems, (await controller.Get()).Items);

        await controller.RemoveItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 0,
            ProductSku = "bar",
            Attributes = _attrSet3,
        });
        expectedCartItems.RemoveAt(3); // bar - attr3
        Assert.Equal(expectedCartItems, (await controller.Get()).Items);

        await controller.RemoveItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 0,
            ProductSku = "foo",
        });
        expectedCartItems.RemoveAt(0); // foo
        Assert.Equal(expectedCartItems, (await controller.Get()).Items);

        await controller.RemoveItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 0,
            ProductSku = "foo",
            Attributes = _attrSet1,
        });
        expectedCartItems.RemoveAt(0); // foo - attr1
        Assert.Equal(expectedCartItems, (await controller.Get()).Items);

        await controller.RemoveItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 0,
            ProductSku = "foo",
            Attributes = _attrSet3,
        });
        expectedCartItems.RemoveAt(0); // foo - attr3
        Assert.Equal(expectedCartItems, (await controller.Get()).Items);
    }

    private ShoppingCartController GetController()
    {
        var mocker = new AutoMocker();
        var mockSession = mocker.GetMock<ISession>();
        mockSession.Setup(session => session.Id).Returns("MockSession");

        var mockContext = MockHelper.CreateMockControllerContextWithUser(mocker);
        mockContext.HttpContext.Session = mockSession.Object;

        var mockContextAccessor = mocker.GetMock<IHttpContextAccessor>();
        mockContextAccessor.Setup(hca => hca.HttpContext).Returns(mockContext.HttpContext);

        mocker.Use(mocker.CreateShoppingCartSerializerInstance());
        mocker.Use(_cartStorage);
        mocker.Use<IHtmlLocalizer<ShoppingCartHelpers>>(new HtmlLocalizer<ShoppingCartHelpers>(new NullHtmlLocalizerFactory()));
        mocker.Use<IShoppingCartHelpers>(mocker.CreateInstance<ShoppingCartHelpers>());

        mocker.Use<IEnumerable<IWorkflowManager>>([]);

        mocker.Use<IShoppingCartService>(mocker.CreateInstance<ShoppingCartService>());
        var controller = mocker.CreateInstance<ShoppingCartController>();
        controller.ControllerContext = mockContext;
        return controller;
    }

    private Task StoreCartAsync(string cartId) =>
        _cartStorage.StoreAsync(new ShoppingCart(new ShoppingCartItem(3, "foo")), cartId);

    private async Task<ShoppingCart> StoreAndRetrieveItemAsync(string sku, int quantity = 7)
    {
        var cartId = Guid.NewGuid().ToString();
        await StoreCartAsync(cartId);

        using var controller = GetController();
        await AddItemAsync(controller, cartId, quantity, sku);

        return await _cartStorage.RetrieveAsync(cartId);
    }

    private static Task<ActionResult> AddItemAsync(
        ShoppingCartController controller,
        string cartId,
        int quantity,
        string sku,
        IDictionary<string, string[]> attributes = null) =>
        controller.AddItem(
            new ShoppingCartLineUpdateModel
            {
                Quantity = quantity,
                ProductSku = sku,
                Attributes = attributes,
            },
            cartId);
}
