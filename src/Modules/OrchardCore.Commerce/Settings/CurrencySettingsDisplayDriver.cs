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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class CurrencySettingsDisplayDriver : SectionDisplayDriver<ISite, CurrencySettings>
{
    public const string GroupId = "commerce";
    private readonly IShellHost _orchardHost;
    private readonly ShellSettings _currentShellSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMoneyService _moneyService;
    private readonly IStringLocalizer T;

    public CurrencySettingsDisplayDriver(
        IShellHost orchardHost,
        ShellSettings currentShellSettings,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IMoneyService moneyService,
        IStringLocalizer<CurrencySettingsDisplayDriver> stringLocalizer)
    {
        _orchardHost = orchardHost;
        _currentShellSettings = currentShellSettings;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _moneyService = moneyService;
        T = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(CurrencySettings section, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCurrencySettings))
        {
            return null;
        }

        var shapes = new List<IDisplayResult>
        {
            Initialize<CurrencySettingsViewModel>("CurrencySettings_Edit", model =>
            {
                model.DefaultCurrency = section.DefaultCurrency ?? _moneyService.DefaultCurrency.CurrencyIsoCode;
                model.CurrentDisplayCurrency = section.CurrentDisplayCurrency ?? _moneyService.DefaultCurrency.CurrencyIsoCode;
                model.Currencies = _moneyService.Currencies
                    .OrderBy(currency => currency.CurrencyIsoCode)
                    .Select(currency => new SelectListItem(
                        currency.CurrencyIsoCode,
                        $"{currency.CurrencyIsoCode} {currency.Symbol} - " +
                        (string.IsNullOrEmpty(currency.EnglishName) ? T["Unspecified"] : T[currency.EnglishName])));
            })
                .Location("Content:5")
                .OnGroup(GroupId),
        };

        return Combine(shapes);
    }

    public override async Task<IDisplayResult> UpdateAsync(CurrencySettings section, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCurrencySettings))
        {
            return null;
        }

        if (context.GroupId == GroupId)
        {
            var model = new CurrencySettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                section.DefaultCurrency = model.DefaultCurrency;
                section.CurrentDisplayCurrency = model.CurrentDisplayCurrency;
            }

            // Reload the tenant to apply the settings.
            await _orchardHost.ReloadShellContextAsync(_currentShellSettings);
        }

        return await EditAsync(section, context);
    }
}
