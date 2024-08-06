using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tax.Drivers;

public class TaxRateSettingsDisplayDriver : SiteDisplayDriver<TaxRateSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;
    private readonly IStringLocalizer T;

    public TaxRateSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IStringLocalizer<TaxRateSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        T = stringLocalizer;
    }

    protected override string SettingsGroupId
        => nameof(TaxRateSettings);

    public override async Task<IDisplayResult> EditAsync(ISite model, TaxRateSettings section, BuildEditorContext context) =>
        _hca.HttpContext?.User is { } user &&
        await _authorizationService.AuthorizeAsync(user, TaxRatePermissions.ManageCustomTaxRates)
            ? Initialize<TaxRateSettings>($"{nameof(TaxRateSettings)}_Edit", model =>
                {
                    model.CopyFrom(section);
                    if (!model.Rates.Any()) model.Rates.Add(new TaxRateSetting());
                })
                .Location(CommonLocationNames.Content)
                .OnGroup(SettingsGroupId)
            : null;

    public override async Task<IDisplayResult> UpdateAsync(ISite model, TaxRateSettings section, UpdateEditorContext context)
    {
        var user = _hca.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, TaxRatePermissions.ManageCustomTaxRates)) return null;

        var settings = new TaxRateSettings();

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        foreach (var rate in settings.Rates)
        {
            Validate(context, rate.DestinationStreetAddress1);
            Validate(context, rate.DestinationStreetAddress2);
            Validate(context, rate.DestinationCity);
            Validate(context, rate.DestinationProvince);
            Validate(context, rate.DestinationPostalCode);
            Validate(context, rate.DestinationRegion);
            Validate(context, rate.VatNumber);
            Validate(context, rate.TaxCode);
        }

        if (context.Updater.ModelState.IsValid)
        {
            section.CopyFrom(settings);

            section.Rates
                .Where(rate =>
                    string.IsNullOrEmpty(rate.DestinationStreetAddress1) &&
                    string.IsNullOrEmpty(rate.DestinationStreetAddress2) &&
                    string.IsNullOrEmpty(rate.DestinationCity) &&
                    string.IsNullOrEmpty(rate.DestinationProvince) &&
                    string.IsNullOrEmpty(rate.DestinationPostalCode) &&
                    string.IsNullOrEmpty(rate.DestinationRegion) &&
                    string.IsNullOrEmpty(rate.VatNumber) &&
                    string.IsNullOrEmpty(rate.TaxCode))
                .ToList()
                .ForEach(rate => section.Rates.Remove(rate));
        }

        return await EditAsync(model, settings, context);
    }

    private void Validate(BuildShapeContext context, string value, [CallerMemberName] string name = "")
    {
        if (string.IsNullOrWhiteSpace(value) || IsValidRegex(value)) return;

        context.Updater.ModelState.AddModelError(
            name,
            T["The \"{0}\" must be empty or a valid regular expression.", name]);
    }

    private static bool IsValidRegex(string pattern)
    {
        // Unfortunately there is no TryParse in the public API so we have to use try-catch.
        try
        {
            Regex.Match(string.Empty, pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
