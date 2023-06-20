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

public class TaxRateSettingsDisplayDriver : SectionDisplayDriver<ISite, TaxRateSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;
    private readonly IStringLocalizer<TaxRateSettingsDisplayDriver> T;

    public TaxRateSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IStringLocalizer<TaxRateSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        T = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(TaxRateSettings section, BuildEditorContext context) =>
        _hca.HttpContext?.User is { } user &&
        await _authorizationService.AuthorizeAsync(user, TaxRatePermissions.ManageCustomTaxRates)
            ? Initialize<TaxRateSettings>($"{nameof(TaxRateSettings)}_Edit", model =>
                {
                    model.MatchTaxRates = section.MatchTaxRates;
                    model.CopyFrom(section);
                    if (!model.Rates.Any()) model.Rates.Add(new TaxRateSetting());
                })
                .Location(CommonLocationNames.Content)
                .OnGroup(nameof(TaxRateSettings))
            : null;

    public override async Task<IDisplayResult> UpdateAsync(TaxRateSettings section, BuildEditorContext context)
    {
        var user = _hca.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, TaxRatePermissions.ManageCustomTaxRates)) return null;

        var model = new TaxRateSettings();

        if (context.GroupId == nameof(TaxRateSettings) &&
            await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            section.MatchTaxRates = model.MatchTaxRates;

            foreach (var rate in model.Rates)
            {
                Validate(context, rate.DestinationStreetAddress1);
                Validate(context, rate.DestinationStreetAddress2);
                Validate(context, rate.DestinationCity);
                Validate(context, rate.DestinationProvince);
                Validate(context, rate.DestinationPostalCode);
                Validate(context, rate.DestinationRegion);

                Validate(context, rate.TaxCode);
            }

            if (context.Updater.ModelState.IsValid)
            {
                section.CopyFrom(model);

                section.Rates
                    .Where(rate =>
                        string.IsNullOrEmpty(rate.DestinationStreetAddress1) &&
                        string.IsNullOrEmpty(rate.DestinationStreetAddress2) &&
                        string.IsNullOrEmpty(rate.DestinationCity) &&
                        string.IsNullOrEmpty(rate.DestinationProvince) &&
                        string.IsNullOrEmpty(rate.DestinationPostalCode) &&
                        string.IsNullOrEmpty(rate.DestinationRegion) &&
                        string.IsNullOrEmpty(rate.TaxCode))
                    .ToList()
                    .ForEach(rate => section.Rates.Remove(rate));
            }
        }

        return await EditAsync(section, context);
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
