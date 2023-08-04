using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Exceptions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ShoppingCartHelpers : IShoppingCartHelpers
{
    private readonly IHttpContextAccessor _hca;
    private readonly IPriceService _priceService;
    private readonly IProductService _productService;
    private readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IHtmlLocalizer<ShoppingCartHelpers> H;

    public ShoppingCartHelpers(
        IHttpContextAccessor hca,
        IPriceService priceService,
        IProductService productService,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents,
        IShoppingCartPersistence shoppingCartPersistence,
        IHtmlLocalizer<ShoppingCartHelpers> localizer)
    {
        _hca = hca;
        _priceService = priceService;
        _productService = productService;
        _shoppingCartEvents = shoppingCartEvents;
        _shoppingCartPersistence = shoppingCartPersistence;
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
                return new ShoppingCartLineViewModel(attributes: item.Attributes.ToDictionary(attr => attr.AttributeName))
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
        IList<Amount> totals = (await CalculateMultipleCurrencyTotalsAsync()).Values.ToList();

        (shipping, billing) = await _hca.GetUserAddressIfNullAsync(shipping, billing);

        foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
        {
            (headers, lines) = await shoppingCartEvent.DisplayingAsync(new ShoppingCartDisplayingEventContext(
                totals, headers, lines, shipping, billing));
            totals = lines.CalculateTotals().ToList();
        }

        // The values are rounded to avoid storing more precision than what the currency supports.
        foreach (var line in lines)
        {
            line.LinePrice = line.LinePrice.GetRounded();
            line.UnitPrice = line.UnitPrice.GetRounded();
        }

        model.Totals.AddRange(totals.Any() ? totals.Round() : new List<Amount> { new(0, lines[0].LinePrice.Currency) });

        model.Headers.AddRange(headers);
        model.Lines.AddRange(lines);

        return model;
    }

    public async Task<Amount?> CalculateSingleCurrencyTotalAsync()
    {
        var totals = await CalculateMultipleCurrencyTotalsAsync();
        return totals.Count > 0 ? totals.Single().Value : null;
    }

    public async Task<IDictionary<string, Amount>> CalculateMultipleCurrencyTotalsAsync()
    {
        // Shopping cart ID is null by default currently.
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        if (currentShoppingCart.Count == 0) return new Dictionary<string, Amount>();

        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService);
        return totals.ToDictionary(total => total.Currency.CurrencyIsoCode);
    }

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

        if (await ShoppingCartItem.GetErrorAsync(parsedLine.ProductSku, parsedLine, H, _priceService) is { } error)
        {
            throw new FrontendException(error);
        }

        return (cart, item);
    }

    public async Task<ShoppingCartLineViewModel> EstimateProductAsync(
        string shoppingCartId,
        ShoppingCartItem item,
        Address shipping = null,
        Address billing = null)
    {
        var sku = item.ProductSku;
        var (cart, _) = await AddItemAndGetCartAsync(shoppingCartId, item);

        return (await CreateShoppingCartViewModelAsync(cart, shipping, billing))
            .Lines
            .Single(line => line.ProductSku == sku);
    }
}
