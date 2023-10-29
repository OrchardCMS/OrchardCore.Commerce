using Lombiq.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq.AutoMock;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Localization;
using OrchardCore.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class ShoppingCartControllerTests
{
    private readonly IShoppingCartPersistence _cartStorage;

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
    private readonly HashSet<IProductAttributeValue> _attrSet1Parsed = new()
    {
        new BooleanProductAttributeValue("ProductPart3.attr1", value: true),
    };
    private readonly HashSet<IProductAttributeValue> _attrSet2Parsed = new()
    {
        new BooleanProductAttributeValue("ProductPart3.attr1", value: false),
    };
    private readonly HashSet<IProductAttributeValue> _attrSet3Parsed = new()
    {
        new BooleanProductAttributeValue("ProductPart3.attr1", value: true),
        new TextProductAttributeValue("ProductPart3.attr2", "bar", "baz"),
    };

    public ShoppingCartControllerTests() => _cartStorage = new FakeCartStorage();

    [Fact]
    public async Task AddExistingItemToCart()
    {
        var cartId = Guid.NewGuid().ToString();
        await StoreCartAsync(cartId: null);
        using var controller = GetController();
        await controller.AddItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 7,
            ProductSku = "foo",
        });
        var cart = await _cartStorage.RetrieveAsync(cartId);

        Assert.Equal(
            new List<ShoppingCartItem> { new(10, "foo") },
            cart.Items);
    }

    [Fact]
    public async Task AddNewItemToCart()
    {
        var cartId = Guid.NewGuid().ToString();
        await StoreCartAsync(cartId);
        using var controller = GetController();
        await controller.AddItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 7,
            ProductSku = "bar",
        });
        var cart = await _cartStorage.RetrieveAsync(cartId);

        Assert.Equal(
            new List<ShoppingCartItem>
            {
                new(3, "foo"),
                new(7, "bar"),
            },
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
        await controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 7, ProductSku = "foo" });
        await controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 8, ProductSku = "foo", Attributes = _attrSet1 });
        await controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 9, ProductSku = "foo", Attributes = _attrSet2 });
        await controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 10, ProductSku = "foo", Attributes = _attrSet3 });
        await controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 11, ProductSku = "bar", Attributes = _attrSet3 });
        await controller.AddItem(new ShoppingCartLineUpdateModel { Quantity = 13, ProductSku = "baz", Attributes = _attrSet3 });
        var cart = await controller.Get();

        Assert.Equal(
            new List<ShoppingCartItem>
            {
                new(9, "foo"),
                new(11, "foo", _attrSet1Parsed),
                new(13, "foo", _attrSet2Parsed),
                new(15, "foo", _attrSet3Parsed),
                new(17, "bar", _attrSet3Parsed),
                new(13, "baz", _attrSet3Parsed),
            },
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

        mocker.Use<IPriceService>(new FakePriceService());
        mocker.Use<IProductService>(new FakeProductService());
        mocker.Use<IEnumerable<IShoppingCartEvents>>(new[] { new FakeShoppingCartEvents() });
        mocker.Use<IEnumerable<IProductAttributeProvider>>(new[] { new ProductAttributeProvider() });
        mocker.Use<IContentDefinitionManager>(new FakeContentDefinitionManager());
        mocker.Use<IMoneyService>(new TestMoneyService());
        mocker.Use<IShoppingCartSerializer>(mocker.CreateInstance<ShoppingCartSerializer>());

        mocker.Use(_cartStorage);
        mocker.Use<IHtmlLocalizer<ShoppingCartHelpers>>(new HtmlLocalizer<ShoppingCartHelpers>(new NullHtmlLocalizerFactory()));
        mocker.Use<IShoppingCartHelpers>(mocker.CreateInstance<ShoppingCartHelpers>());

        mocker.Use<IEnumerable<IWorkflowManager>>(Array.Empty<IWorkflowManager>());
        var controller = mocker.CreateInstance<ShoppingCartController>();
        controller.ControllerContext = mockContext;
        return controller;
    }

    private Task StoreCartAsync(string cartId) =>
        _cartStorage.StoreAsync(new ShoppingCart(new ShoppingCartItem(3, "foo")), cartId);
}
