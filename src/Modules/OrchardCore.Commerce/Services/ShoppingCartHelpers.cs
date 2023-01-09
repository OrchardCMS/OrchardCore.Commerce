using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
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

        var model = new ShoppingCartViewModel { Id = shoppingCartId };

        IList<LocalizedHtmlString> headers = new[]
        {
            H["Quantity"],
            H["Product"],
            H["Price"],
            H["Action"],
        };
        IList<Amount> totals = (await CalculateMultipleCurrencyTotalsAsync()).Values.ToList();

        if ((shipping == null || billing == null) &&
            _hca.HttpContext is { } httpContext &&
            await httpContext.GetUserAddressAsync() is { } userAddresses)
        {
            shipping ??= userAddresses.ShippingAddress.Address;
            billing ??= userAddresses.BillingAddress.Address;
        }

        foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
        {
            (headers, lines) = await shoppingCartEvent.DisplayingAsync(new ShoppingCartDisplayingEventContext(
                totals, headers, lines, shipping, billing));
            totals = lines.CalculateTotals().ToList();
        }

        model.Totals.AddRange(totals);
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
}
