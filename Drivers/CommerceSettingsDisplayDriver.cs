using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers
{
    public class CommerceSettingsDisplayDriver : SectionDisplayDriver<ISite, CommerceSettings>
    {
        public const string GroupId = "commerce";
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IShellHost _orchardHost;
        private readonly ShellSettings _currentShellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMoneyService _moneyService;

        public CommerceSettingsDisplayDriver(
            IDataProtectionProvider dataProtectionProvider,
            IShellHost orchardHost,
            ShellSettings currentShellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IMoneyService moneyService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _orchardHost = orchardHost;
            _currentShellSettings = currentShellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _moneyService = moneyService;
        }

        public override async Task<IDisplayResult> EditAsync(CommerceSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCommerceSettings))
            {
                return null;
            }

            var shapes = new List<IDisplayResult>
            {
                Initialize<CommerceSettingsViewModel>("CommerceSettings_Edit", model =>
                {
                    model.DefaultCurrency = section.DefaultCurrency;
                    model.Currencies = _moneyService.Currencies;
                }).Location("Content:5").OnGroup(GroupId)
            };

            return Combine(shapes);
        }

        public override async Task<IDisplayResult> UpdateAsync(CommerceSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCommerceSettings))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new CommerceSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    section.DefaultCurrency = model.DefaultCurrency;
                }

                // Reload the tenant to apply the settings
                await _orchardHost.ReloadShellContextAsync(_currentShellSettings);
            }

            return await EditAsync(section, context);
        }
    }
}
