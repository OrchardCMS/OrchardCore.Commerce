using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ShoppingCartHelpers : IShoppingCartHelpers
{
    private readonly IPriceService _priceService;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<ShoppingCartController> T;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;

    public ShoppingCartHelpers(
        IPriceService priceService,
        INotifier notifier,
        IHtmlLocalizer<ShoppingCartController> localizer,
        IPriceSelectionStrategy priceSelectionStrategy,
        IShoppingCartPersistence shoppingCartPersistence)
    {
        _priceService = priceService;
        _notifier = notifier;
        T = localizer;
        _priceSelectionStrategy = priceSelectionStrategy;
        _shoppingCartPersistence = shoppingCartPersistence;
    }

    public ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line) =>
        cart.Lines.FirstOrDefault(viewModel => ShoppingCartLineViewModel.IsSameProductAs(viewModel, line));

    public async Task<ShoppingCartItem> ValidateParsedCartLineAsync(
        ShoppingCartLineUpdateModel line,
        ShoppingCartItem parsedLine)
    {
        var sku = line.ProductSku;

        if (parsedLine is null)
        {
            await _notifier.AddAsync(NotifyType.Error, T["Product with SKU {0} not found.", sku]);
            return null;
        }

        parsedLine = (await _priceService.AddPricesAsync(new[] { parsedLine })).Single();
        if (parsedLine.Prices.Any()) return parsedLine;

        await _notifier.AddAsync(
            NotifyType.Error,
            T["Can't add product {0} because it doesn't have a price, or its currency doesn't match the current display currency.", sku]);
        return null;
    }

    public async Task<Amount?> CalculateSingleCurrencyTotalAsync()
    {
        var totals = await CalculateMultipleCurrencyTotalsAsync();
        return totals.Single().Value;
    }

    public async Task<IDictionary<string, Amount>> CalculateMultipleCurrencyTotalsAsync()
    {
        // Shopping cart ID is null by default currently.
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        if (currentShoppingCart.Count == 0) return new Dictionary<string, Amount>();

        var totals = await currentShoppingCart.CalculateTotalsAsync(_priceService, _priceSelectionStrategy);
        return totals.ToDictionary(total => total.Currency.CurrencyIsoCode);
    }
}
