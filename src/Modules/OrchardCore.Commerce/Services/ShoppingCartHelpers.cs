using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ShoppingCartHelpers : IShoppingCartHelpers
{
    private readonly IPriceService _priceService;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;

    public ShoppingCartHelpers(
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy,
        IShoppingCartPersistence shoppingCartPersistence)
    {
        _priceService = priceService;
        _priceSelectionStrategy = priceSelectionStrategy;
        _shoppingCartPersistence = shoppingCartPersistence;
    }

    public ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line) =>
        cart.Lines.FirstOrDefault(viewModel => ShoppingCartLineViewModel.IsSameProductAs(viewModel, line));

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
