using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Settings;

public class CheckoutAddressSettingsDisplayDriver : SiteDisplayDriver<CheckoutAddressSettings>
{
    public const string GroupId = "CheckoutAddressSettings";

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;

    public CheckoutAddressSettingsDisplayDriver(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _hca = hca;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult?> EditAsync(ISite model, CheckoutAddressSettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync())
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<CheckoutAddressSettings>(
                $"{nameof(CheckoutAddressSettings)}_Edit",
                model => model.ShouldIgnoreAddress = section.ShouldIgnoreAddress)
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult?> UpdateAsync(ISite model, CheckoutAddressSettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<CheckoutAddressSettings>(Prefix, AuthorizeAsync) is { } viewModel)
        {
            section.ShouldIgnoreAddress = viewModel.ShouldIgnoreAddress;

            // Release the tenant to apply settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(_hca.HttpContext?.User, Permissions.ManageOrders);
}
