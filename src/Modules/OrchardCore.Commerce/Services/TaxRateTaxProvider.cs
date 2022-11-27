using GraphQL;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class TaxRateTaxProvider : ITaxProvider
{
    private readonly IHttpContextAccessor _hca;
    private readonly ISiteService _siteService;
    private readonly IUserService _userService;

    // Give plenty of room for other providers of higher or lower priority.
    public int Order => int.MaxValue / 2;

    public TaxRateTaxProvider(
        IHttpContextAccessor hca,
        ISiteService siteService,
        IUserService userService)
    {
        _hca = hca;
        _siteService = siteService;
        _userService = userService;
    }

    public async Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var taxRates = siteSettings.As<TaxRateSettings>();

        var address = _userService.GetUserSetting<UserAddressesPart>(await _userService.GetCurrentFullUserAsync(_hca))?
            .ShippingAddress
            .Address ?? new Address();

        var items = model.Items.AsList();

        var updatedItems = items
            .Select(item =>
            {
                var taxCode = item.As<TaxPart>()?.ProductTaxCode?.Text;

                var taxRate = MatchTaxRate(taxRates.Rates, address, taxCode)?.TaxRate ?? 1;
                return item with { UnitPrice = item.UnitPrice * taxRate };
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

            var address = _userService.GetUserSetting<UserAddressesPart>(await _userService.GetCurrentFullUserAsync(_hca))?
                .ShippingAddress
                .Address ?? new Address();

            return items.Count(item =>
                item.As<TaxPart>()?.ProductTaxCode?.Text is { } taxCode &&
                MatchTaxRate(taxRates.Rates, address, taxCode) != null);
        });

    private static bool IsMatching(string pattern, string text) =>
        !string.IsNullOrEmpty(pattern) && (text ?? string.Empty).RegexIsMatch(pattern);

    private static TaxRateSetting MatchTaxRate(
        IEnumerable<TaxRateSetting> taxRates,
        Address destinationAddress,
        string taxCode)
    {
        foreach (var rate in taxRates)
        {
            if (IsMatching(rate.DestinationStreetAddress1, destinationAddress.StreetAddress1) &&
                IsMatching(rate.DestinationStreetAddress2, destinationAddress.StreetAddress2) &&
                IsMatching(rate.DestinationCity, destinationAddress.City) &&
                IsMatching(rate.DestinationProvince, destinationAddress.Province) &&
                IsMatching(rate.DestinationPostalCode, destinationAddress.PostalCode) &&
                IsMatching(rate.DestinationRegion, destinationAddress.Region) &&
                IsMatching(rate.TaxCode, taxCode))
            {
                return rate;
            }
        }

        return null;
    }
}
