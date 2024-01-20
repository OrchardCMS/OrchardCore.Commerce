using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Drivers;

public class ExactlySettingsDisplayDriver : SectionDisplayDriver<ISite, ExactlySettings>
{
    public const string EditorGroupId = "Exactly";

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;
    private readonly ExactlySettings _ssoSettings;

    public ExactlySettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IOptionsSnapshot<ExactlySettings> ssoSettings)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        _ssoSettings = ssoSettings.Value;
    }

    public override async Task<IDisplayResult> EditAsync(ExactlySettings section, BuildEditorContext context) =>
        await AuthorizeAsync()
            ? Initialize<ExactlySettings>($"{nameof(ExactlySettings)}_Edit", _ssoSettings.CopyTo)
                .PlaceInContent()
                .OnGroup(EditorGroupId)
            : null;

    public override async Task<IDisplayResult> UpdateAsync(ExactlySettings section, BuildEditorContext context)
    {
        var viewModel = new ExactlySettings();

        if (context.GroupId == EditorGroupId &&
            await AuthorizeAsync() &&
            await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            viewModel.CopyTo(section);
        }

        return await EditAsync(section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeCurrentUserAsync(_hca.HttpContext, Permissions.ManageExactlySettings);
}
