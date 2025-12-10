using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class TaxRateTaxProvider : ITaxProvider
{
    private readonly ISiteService _siteService;

    // Give plenty of room for other providers of higher or lower priority.
    public int Order => int.MaxValue / 2;

    public TaxRateTaxProvider(ISiteService siteService) => _siteService = siteService;

    public async Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var taxRates = siteSettings.As<TaxRateSettings>();

        var items = model.Items.AsList();

        var updatedItems = items
            .Select(item =>
            {
                var taxRate = MatchTaxRate(
                    taxRates.Rates,
                    model.ShippingAddress,
                    item.Content.As<TaxPart>()?.ProductTaxCode?.Text,
                    model.VatNumber,
                    model.IsCorporation);
                var multiplier = (taxRate / 100m) + 1;
                return item with { UnitPrice = item.UnitPrice * multiplier };
            })
            .ToList();

        return model with { Items = updatedItems };
    }

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        ITaxProvider.AllOrNoneAsync(model, async items =>
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var taxRates = siteSettings.As<TaxRateSettings>();
            if (taxRates?.Rates.Any() != true) return 0;

            return items.Count(item =>
                MatchTaxRate(
                    taxRates.Rates,
                    model.ShippingAddress,
                    item.Content.As<TaxPart>()?.ProductTaxCode?.Text,
                    model.VatNumber,
                    model.IsCorporation) > 0);
        });

    private static bool IsMatchingOrEmptyPattern(string pattern, string text) =>
        string.IsNullOrEmpty(pattern) || (text ?? string.Empty).RegexIsMatch(pattern);

    private static decimal MatchTaxRate(
        IEnumerable<TaxRateSetting> taxRates,
        Address destinationAddress,
        string taxCode,
        string vatNumber,
        bool buyerIsCorporation)
    {
        destinationAddress ??= new Address();

        var matchingTaxRate = taxRates.FirstOrDefault(rate =>
        {
            var shouldMatchTaxRate = rate.IsCorporation switch
            {
                MatchTaxRates.Checked => buyerIsCorporation,
                MatchTaxRates.Unchecked => !buyerIsCorporation,
                MatchTaxRates.Ignored => true,
                _ => true,
            };

            return shouldMatchTaxRate &&
                IsMatchingOrEmptyPattern(rate.DestinationStreetAddress1, destinationAddress.StreetAddress1) &&
                IsMatchingOrEmptyPattern(rate.DestinationStreetAddress2, destinationAddress.StreetAddress2) &&
                IsMatchingOrEmptyPattern(rate.DestinationCity, destinationAddress.City) &&
                IsMatchingOrEmptyPattern(rate.DestinationProvince, destinationAddress.Province) &&
                IsMatchingOrEmptyPattern(rate.DestinationPostalCode, destinationAddress.PostalCode) &&
                IsMatchingOrEmptyPattern(rate.DestinationRegion, destinationAddress.Region) &&
                IsMatchingOrEmptyPattern(rate.VatNumber, vatNumber) &&
                IsMatchingOrEmptyPattern(rate.TaxCode, taxCode);
        });

        return matchingTaxRate?.TaxRate ?? 0;
    }
}
