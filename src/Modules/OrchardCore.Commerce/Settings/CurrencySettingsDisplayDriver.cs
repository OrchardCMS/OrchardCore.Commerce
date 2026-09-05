using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class CurrencySettingsDisplayDriver : SiteDisplayDriver<CurrencySettings>
{
    public const string GroupId = "commerce";
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMoneyService _moneyService;
    private readonly IStringLocalizer T;

    public CurrencySettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IMoneyService moneyService,
        IStringLocalizer<CurrencySettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _moneyService = moneyService;
        T = stringLocalizer;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite model, CurrencySettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync())
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<CurrencySettingsViewModel>("CurrencySettings_Edit", model =>
        {
            model.DefaultCurrency = section.DefaultCurrency ?? _moneyService.DefaultCurrency.CurrencyIsoCode;
            model.CurrentDisplayCurrency = section.CurrentDisplayCurrency ?? _moneyService.DefaultCurrency.CurrencyIsoCode;
            model.Currencies = _moneyService.Currencies
                .OrderBy(currency => currency.CurrencyIsoCode)
                .Select(currency => new SelectListItem(
                    currency.CurrencyIsoCode,
                    $"{currency.CurrencyIsoCode} {currency.Symbol} - " +
                    (string.IsNullOrEmpty(currency.EnglishName) ? T["Unspecified"] : T[currency.EnglishName])));
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, CurrencySettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<CurrencySettingsViewModel>(Prefix, AuthorizeAsync) is { } viewModel)
        {
            section.DefaultCurrency = viewModel.DefaultCurrency;
            section.CurrentDisplayCurrency = viewModel.CurrentDisplayCurrency;

            // Reload the tenant to apply the settings.
            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageCurrencySettings);
}
