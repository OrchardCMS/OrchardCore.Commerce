using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Exceptions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ShoppingCartHelpers : IShoppingCartHelpers
{
    private readonly IHttpContextAccessor _hca;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly IPriceService _priceService;
    private readonly IEnumerable<IProductAttributeProvider> _productAttributeProviders;
    private readonly IEnumerable<IProductEstimationContextUpdater> _productEstimationContextUpdaters;
    private readonly IProductService _productService;
    private readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;
    private readonly IHtmlLocalizer<ShoppingCartHelpers> H;

    [SuppressMessage(
        "Major Code Smell",
        "S107:Methods should not have too many parameters",
        Justification = "This service ties together many cart-related features.")]
    public ShoppingCartHelpers(
        IHttpContextAccessor hca,
        IPriceSelectionStrategy priceSelectionStrategy,
        IPriceService priceService,
        IEnumerable<IProductAttributeProvider> productAttributeProviders,
        IEnumerable<IProductEstimationContextUpdater> productEstimationContextUpdaters,
        IProductService productService,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents,
        IShoppingCartPersistence shoppingCartPersistence,
        IShoppingCartSerializer shoppingCartSerializer,
        IHtmlLocalizer<ShoppingCartHelpers> localizer)
    {
        _hca = hca;
        _priceSelectionStrategy = priceSelectionStrategy;
        _priceService = priceService;
        _productAttributeProviders = productAttributeProviders;
        _productEstimationContextUpdaters = productEstimationContextUpdaters;
        _productService = productService;
        _shoppingCartEvents = shoppingCartEvents;
        _shoppingCartPersistence = shoppingCartPersistence;
        _shoppingCartSerializer = shoppingCartSerializer;
        H = localizer;
    }

    public async Task<ShoppingCartViewModel> CreateShoppingCartViewModelAsync(
        string shoppingCartId,
        Address shipping = null,
        Address billing = null)
    {
        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        return await CreateShoppingCartViewModelAsync(cart, shipping, billing);
    }

    private async Task<ShoppingCartViewModel> CreateShoppingCartViewModelAsync(
        ShoppingCart cart,
        Address shipping = null,
        Address billing = null)
    {
        var products = await _productService.GetProductDictionaryAsync(cart.Items.Select(line => line.ProductSku));
        var items = await _priceService.AddPricesAsync(cart.Items);

        IList<ShoppingCartLineViewModel> lines = items
            .Select(item =>
            {
                var product = products[item.ProductSku];
                var price = _priceService.SelectPrice(item.Prices);

                var attributes = item.HasRawAttributes()
                    ? _shoppingCartSerializer.PostProcessAttributes(item.Attributes, product)
                    : item.Attributes;

                return new ShoppingCartLineViewModel(attributes: attributes.ToDictionary(attr => attr.AttributeName))
                {
                    Quantity = item.Quantity,
                    Product = product,
                    ProductSku = item.ProductSku,
                    ProductName = product.ContentItem.DisplayText,
                    UnitPrice = price,
                    LinePrice = item.Quantity * price,
                };
            })
            .ToList();

        if (!lines.Any()) return null;

        var model = new ShoppingCartViewModel { Id = cart.Id };

        IList<LocalizedHtmlString> headers = new[]
        {
            H["Quantity"],
            H["Product"],
            H["Price"],
            H["Action"],
        };
        IList<Amount> totals = (await CalculateMultipleCurrencyTotalsAsync(cart)).Values.ToList();

        (shipping, billing) = await _hca.GetUserAddressIfNullAsync(shipping, billing);

        foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
        {
            (headers, lines) = await shoppingCartEvent.DisplayingAsync(new ShoppingCartDisplayingEventContext(
                totals, headers, lines, shipping, billing));
            totals = lines.CalculateTotals().ToList();
        }

        // Checkout should not be possible if any of the items are unpurchasable.
        var cannotCheckout = lines
            .Where(line => line.Product.As<PriceVariantsPart>() is null)
            .Select(line => line.Product.As<InventoryPart>())
            .Where(inventoryPart => inventoryPart is not null)
            .Any(inventoryPart => inventoryPart.Inventory[inventoryPart.ProductSku] < 1 &&
                !inventoryPart.AllowsBackOrder.Value &&
                !inventoryPart.IgnoreInventory.Value);

        foreach (var line in lines)
        {
            // The values are rounded to avoid storing more precision than what the currency supports.
            line.LinePrice = line.LinePrice.GetRounded();
            line.UnitPrice = line.UnitPrice.GetRounded();

            var productPart = line.Product.ContentItem.As<ProductPart>();
            var priceVariantsPart = productPart.As<PriceVariantsPart>();
            if (priceVariantsPart is not null && line.Attributes.Any())
            {
                var item = new ShoppingCartItem(line.Quantity, line.ProductSku, line.Attributes.Values);
                var fullSku = _productService.GetOrderFullSku(item, productPart);

                var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? productPart.Sku : fullSku;
                var inventoryPart = productPart.As<InventoryPart>();
                var relevantInventory = inventoryPart.Inventory.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

                cannotCheckout = relevantInventory.Value < 1 &&
                    !inventoryPart.AllowsBackOrder.Value &&
                    !inventoryPart.IgnoreInventory.Value;
            }
        }

        model.IsInvalid = cannotCheckout;

        model.Totals.AddRange(totals.Any() ? totals.Round() : new List<Amount> { new(0, lines[0].LinePrice.Currency) });

        model.Headers.AddRange(headers);
        model.Lines.AddRange(lines);

        return model;
    }

    public Task<ShoppingCart> RetrieveAsync(string shoppingCartId) =>
        _shoppingCartPersistence.RetrieveAsync(shoppingCartId);

    public async Task UpdateAsync(string shoppingCartId, Func<ShoppingCart, Task> updateTask)
    {
        var cart = await RetrieveAsync(shoppingCartId);
        await updateTask(cart);
        await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
    }

    public async Task<Amount?> CalculateSingleCurrencyTotalAsync(ShoppingCart cart)
    {
        var totals = await CalculateMultipleCurrencyTotalsAsync(cart);
        return totals.Count > 0 ? totals.Single().Value : null;
    }

    public async Task<IDictionary<string, Amount>> CalculateMultipleCurrencyTotalsAsync(ShoppingCart cart) =>
        cart.Count == 0
            ? new Dictionary<string, Amount>()
            : (await cart.CalculateTotalsAsync(_priceService)).ToDictionary(total => total.Currency.CurrencyIsoCode);

    public async Task<ShoppingCartItem> AddToCartAsync(
        string shoppingCartId,
        ShoppingCartItem item,
        bool storeIfOk = false)
    {
        var (cart, parsedLine) = await AddItemAndGetCartAsync(shoppingCartId, item);
        if (storeIfOk) await _shoppingCartPersistence.StoreAsync(cart, shoppingCartId);
        return parsedLine;
    }

    private async Task<(ShoppingCart Cart, ShoppingCartItem Item)> AddItemAndGetCartAsync(
        string shoppingCartId,
        ShoppingCartItem item)
    {
        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId);
        var parsedLine = cart.AddItem(item);

        foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
        {
            if (await shoppingCartEvent.VerifyingItemAsync(parsedLine) is { } errorMessage)
            {
                throw new FrontendException(errorMessage);
            }
        }

        if (await GetErrorAsync(parsedLine.ProductSku, parsedLine) is { } error)
        {
            throw new FrontendException(error);
        }

        return (cart, item);
    }

    public Task<ShoppingCartLineViewModel> EstimateProductAsync(
        string shoppingCartId,
        string sku,
        Address shipping = null,
        Address billing = null) =>
        EstimateProductAsync(new ProductEstimationContext(
            shoppingCartId,
            new ShoppingCartItem(quantity: 1, productSku: sku),
            shipping,
            billing));

    private async Task<ShoppingCartLineViewModel> EstimateProductAsync(ProductEstimationContext context)
    {
        foreach (var updater in _productEstimationContextUpdaters)
        {
            if (await updater.IsApplicableAsync(context))
            {
                context = await updater.UpdateAsync(context) ?? context;
            }
        }

        var (cart, _) = await AddItemAndGetCartAsync(context.ShoppingCartId, context.ShoppingCartItem);

        return (await CreateShoppingCartViewModelAsync(cart, context.ShippingAddress, context.BillingAddress))
            .Lines
            .FirstOrDefault(line => line.ProductSku == context.ShoppingCartItem.ProductSku);
    }

    private async Task<LocalizedHtmlString> GetErrorAsync(string sku, ShoppingCartItem item)
    {
        if (item is null)
        {
            return H["Product with SKU {0} not found.", sku];
        }

        item = (await _priceService.AddPricesAsync(new[] { item })).Single();

        return item.Prices.Any()
            ? null
            : H["Can't add product {0} because it doesn't have a price, or its currency doesn't match the current display currency.", sku];
    }

    public async Task<IList<OrderLineItem>> CreateOrderLineItemsAsync(ShoppingCart shoppingCart)
    {
        static string TrimSku(ShoppingCartItem item) => item.ProductSku.Split('-')[0];

        var contentItems = (await _productService.GetProductsAsync(shoppingCart.Items.Select(TrimSku)))
            .ToDictionary(item => item.Sku, item => item.ContentItem);

        return await shoppingCart.Items.AwaitEachAsync(async item =>
        {
            item = await _priceService.AddPriceAsync(item);
            var price = _priceSelectionStrategy.SelectPrice(item.Prices);
            var fullSku = _productService.GetOrderFullSku(item, await _productService.GetProductAsync(item.ProductSku));

            var selectedAttributes = _productAttributeProviders
                .Select(provider => provider.GetSelectedAttributes(item.Attributes))
                .Where(attributesByType => attributesByType.Any())
                .ToDictionary(attributesByType => attributesByType.Keys.First(), attributesByType => attributesByType.Values.First());

            return new OrderLineItem(
                item.Quantity,
                item.ProductSku,
                fullSku,
                price,
                item.Quantity * price,
                contentItems[TrimSku(item)].ContentItemVersionId,
                item.Attributes,
                selectedAttributes);
        });
    }
}
