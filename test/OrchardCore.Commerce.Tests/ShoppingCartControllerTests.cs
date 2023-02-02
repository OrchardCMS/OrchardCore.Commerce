using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ISession = YesSql.ISession;

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
        await _cartStorage.StoreAsync(new ShoppingCart(new ShoppingCartItem(3, "foo")));
        using var controller = GetController();
        await controller.AddItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 7,
            ProductSku = "foo",
        });
        var cart = await _cartStorage.RetrieveAsync();

        Assert.Equal(
            new List<ShoppingCartItem> { new(10, "foo") },
            cart.Items);
    }

    [Fact]
    public async Task AddNewItemToCart()
    {
        await _cartStorage.StoreAsync(new ShoppingCart(new ShoppingCartItem(3, "foo")));
        using var controller = GetController();
        await controller.AddItem(new ShoppingCartLineUpdateModel
        {
            Quantity = 7,
            ProductSku = "bar",
        });
        var cart = await _cartStorage.RetrieveAsync();

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
        var priceService = new FakePriceService();
        var productService = new FakeProductService();
        var shoppingCartEvents = new[] { new FakeShoppingCartEvents() };

        return new(
            shoppingCartPersistence: _cartStorage,
            notifier: null,
            services: new OrchardServices<ShoppingCartController>(
                new Lazy<IAuthorizationService>(() => null),
                new Lazy<IClock>(() => null),
                new Lazy<IContentHandleManager>(() => null),
                new Lazy<IContentManager>(() => null),
                new Lazy<IHttpContextAccessor>(() => null),
                new Lazy<ILogger<ShoppingCartController>>(() => null),
                new Lazy<ISession>(() => null),
                new Lazy<ISiteService>(() => null),
                new Lazy<IStringLocalizer<ShoppingCartController>>(GetStringLocalizer<ShoppingCartController>()),
                new Lazy<IHtmlLocalizer<ShoppingCartController>>(GetHtmlLocalizer<ShoppingCartController>),
                new Lazy<UserManager<IUser>>(() => null)
            ),
            priceService: priceService,
            shoppingCartHelpers: new ShoppingCartHelpers(
                hca: null,
                priceService,
                productService,
                shoppingCartEvents,
                _cartStorage,
                GetHtmlLocalizer<ShoppingCartHelpers>()
            ),
            shoppingCartSerializer: new ShoppingCartSerializer(
                attributeProviders: new[] { new ProductAttributeProvider() },
                contentDefinitionManager: new FakeContentDefinitionManager(),
                moneyService: new TestMoneyService(),
                productService),
            workflowManager: null,
            shapeFactory: null,
            shoppingCartEvents: shoppingCartEvents);
    }

    private static IStringLocalizer<T> GetStringLocalizer<T>() => new StringLocalizer<T>(new NullStringLocalizerFactory());
    private static IHtmlLocalizer<T> GetHtmlLocalizer<T>() => new HtmlLocalizer<T>(new NullHtmlLocalizerFactory());
}
