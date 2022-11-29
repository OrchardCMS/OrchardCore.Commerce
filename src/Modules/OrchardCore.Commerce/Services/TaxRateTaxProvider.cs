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
                var taxRate = MatchTaxRate(taxRates.Rates, address, item.As<TaxPart>()?.ProductTaxCode?.Text);
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

            var address = _userService.GetUserSetting<UserAddressesPart>(await _userService.GetCurrentFullUserAsync(_hca))?
                .ShippingAddress
                .Address ?? new Address();

            return items.Count(item =>
                MatchTaxRate(taxRates.Rates, address, item.As<TaxPart>()?.ProductTaxCode?.Text) > 0);
        });

    private static bool IsMatchingOrEmptyPattern(string pattern, string text) =>
        string.IsNullOrEmpty(pattern) || (text ?? string.Empty).RegexIsMatch(pattern);

    private static decimal MatchTaxRate(
        IEnumerable<TaxRateSetting> taxRates,
        Address destinationAddress,
        string taxCode)
    {
        foreach (var rate in taxRates)
        {
            if (IsMatchingOrEmptyPattern(rate.DestinationStreetAddress1, destinationAddress.StreetAddress1) &&
                IsMatchingOrEmptyPattern(rate.DestinationStreetAddress2, destinationAddress.StreetAddress2) &&
                IsMatchingOrEmptyPattern(rate.DestinationCity, destinationAddress.City) &&
                IsMatchingOrEmptyPattern(rate.DestinationProvince, destinationAddress.Province) &&
                IsMatchingOrEmptyPattern(rate.DestinationPostalCode, destinationAddress.PostalCode) &&
                IsMatchingOrEmptyPattern(rate.DestinationRegion, destinationAddress.Region) &&
                IsMatchingOrEmptyPattern(rate.TaxCode, taxCode))
            {
                return rate.TaxRate;
            }
        }

        return 0;
    }
}
