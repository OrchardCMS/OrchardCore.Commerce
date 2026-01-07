using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class LocalTaxProvider : ITaxProvider
{
    private readonly ISiteService _siteService;

    // This is the simplest, lowest priority option and if anything else is applicable it should supersede this.
    public int Order => int.MaxValue;

    public LocalTaxProvider(ISiteService siteService) => _siteService = siteService;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model)
    {
        var items = model.Items.AsList();

        var updatedTotals = model
            .TotalsByCurrency
            .Select(total =>
            {
                var currency = total.Currency.CurrencyIsoCode;
                return items
                    .Where(item => item.Subtotal.Currency.CurrencyIsoCode == currency)
                    .Select(item => item.Subtotal.WithTax(item.Content))
                    .Sum();
            });

        return Task.FromResult(model with
        {
            Items = items.Select(item => item with { UnitPrice = item.UnitPrice.WithTax(item.Content) }),
            TotalsByCurrency = updatedTotals,
        });
    }

    public async Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        (await _siteService.GetSettingsAsync<TaxSettings>())?.IgnoreAllOrNone == true ||
        await ITaxProvider.AllOrNoneAsync(model, items => Task.FromResult(HasTaxRate(items)));

    private static int HasTaxRate(IList<PromotionAndTaxProviderContextLineItem> items) => items
        .SelectWhere(item => item.Content.ContentItem.As<TaxPart>())
        .Count(taxPart => taxPart.TaxRate.Value == 0 || (taxPart.GrossPrice.Amount.IsValid && taxPart.TaxRate.Value > 0));
}
