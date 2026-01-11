#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tax.Drivers;

public class TaxSettingsDisplayDriver : SiteDisplayDriver<TaxSettings>
{
    public const string GroupId = nameof(TaxSettings);

    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;

    public TaxSettingsDisplayDriver(
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService)
    {
        _hca = hca;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult?> EditAsync(ISite model, TaxSettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync())
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<TaxSettings>(
                $"{nameof(TaxSettings)}_Edit",
                model => model.IgnoreAllOrNone = section.IgnoreAllOrNone)
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult?> UpdateAsync(ISite model, TaxSettings section, UpdateEditorContext context)
    {
        if (!await AuthorizeAsync())
        {
            return null;
        }

        if (await context.CreateModelMaybeAsync<TaxSettings>(Prefix, AuthorizeAsync) is { } viewModel)
        {
            section.IgnoreAllOrNone = viewModel.IgnoreAllOrNone;
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(_hca.HttpContext?.User, TaxRatePermissions.ManageTaxSettings);
}
