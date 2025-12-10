using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.Commerce.Tax.Services;
using OrchardCore.Commerce.Tax.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tax.Drivers;

public class TaxRateSettingsDisplayDriver : SiteDisplayDriver<TaxRateSettings>, ITaxRateSettingsHeaderProvider
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;
    private readonly IStringLocalizer T;

    public Lazy<IReadOnlyDictionary<string, string>> HeaderLabelsLazy { get; init; }

    public IReadOnlyDictionary<string, string> HeaderLabels => HeaderLabelsLazy.Value;

    public TaxRateSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IStringLocalizer<TaxRateSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        T = stringLocalizer;

        HeaderLabelsLazy = new(() => new Dictionary<string, string>
        {
            [nameof(TaxRateSetting.DestinationStreetAddress1)] = T["First street address"],
            [nameof(TaxRateSetting.DestinationStreetAddress2)] = T["Second street address"],
            [nameof(TaxRateSetting.DestinationCity)] = T["City"],
            [nameof(TaxRateSetting.DestinationProvince)] = T["State or province code"],
            [nameof(TaxRateSetting.DestinationPostalCode)] = T["Postal code"],
            [nameof(TaxRateSetting.DestinationRegion)] = T["Country or region code"],
            [nameof(TaxRateSetting.VatNumber)] = T["Tax code"],
            [nameof(TaxRateSetting.TaxCode)] = T["VAT number"],
            [nameof(TaxRateSetting.TaxRate)] = T["Tax rate (%)"],
            [nameof(TaxRateSetting.IsCorporation)] = T["Is Corporation"],
        });
    }

    protected override string SettingsGroupId
        => nameof(TaxRateSettings);

    public override async Task<IDisplayResult> EditAsync(ISite model, TaxRateSettings section, BuildEditorContext context) =>
        await AuthorizeAsync()
            ? Initialize<TaxRateSettingsViewModel>($"{nameof(TaxRateSettings)}_Edit", model =>
                {
                    if (!section.Rates.Any()) section.Rates.Add(new());
                    model.CopyFrom(section);
                })
                .Location(CommonLocationNames.Content)
                .OnGroup(SettingsGroupId)
            : null;

    public override async Task<IDisplayResult> UpdateAsync(ISite model, TaxRateSettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<TaxRateSettingsViewModel>(Prefix, AuthorizeAsync) is not { } viewModel ||
            _hca.HttpContext?.Request.HasFormContentType != true)
        {
            return null;
        }

        // Parse and update rates.
        viewModel.Rates.SetItems(JsonSerializer
            .Deserialize<IEnumerable<TaxRateSetting>>(viewModel.RatesJson, JOptions.CamelCase)
            .Where(rate => !rate.IsEmpty));

        // Show error if any string entries are invalid RegEx.
        for (var i = 0; i < viewModel.Rates.Count; i++)
        {
            var rate = viewModel.Rates[i];
            Validate(context, i, rate.DestinationStreetAddress1, nameof(TaxRateSetting.DestinationStreetAddress1));
            Validate(context, i, rate.DestinationStreetAddress2, nameof(TaxRateSetting.DestinationStreetAddress2));
            Validate(context, i, rate.DestinationCity, nameof(TaxRateSetting.DestinationCity));
            Validate(context, i, rate.DestinationProvince, nameof(TaxRateSetting.DestinationProvince));
            Validate(context, i, rate.DestinationPostalCode, nameof(TaxRateSetting.DestinationPostalCode));
            Validate(context, i, rate.DestinationRegion, nameof(TaxRateSetting.DestinationRegion));
            Validate(context, i, rate.VatNumber, nameof(TaxRateSetting.VatNumber));
            Validate(context, i, rate.TaxCode, nameof(TaxRateSetting.TaxCode));
        }

        section.CopyFrom(viewModel);
        return await EditAsync(model, viewModel, context);
    }

    private void Validate(BuildShapeContext context, int index, string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || IsValidRegex(value)) return;

        var label = HeaderLabels[name];
        context.Updater.ModelState.AddModelError(
            name,
            T["The value in column \"{0}\" in row {1} must be empty or a valid regular expression.", label, index + 1]);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(_hca.HttpContext?.User, TaxRatePermissions.ManageCustomTaxRates);

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
