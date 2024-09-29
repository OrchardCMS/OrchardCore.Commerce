using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.ContentFields.Events;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Events;

public class UserAddressFieldEvents : IAddressFieldEvents
{
    private readonly IHttpContextAccessor _hca;
    private readonly IUserService _userService;
    public UserAddressFieldEvents(IHttpContextAccessor hca, IUserService userService)
    {
        _hca = hca;
        _userService = userService;
    }

    public async Task UpdatingAsync(
        AddressFieldViewModel viewModel,
        AddressField field,
        IUpdateModel updater,
        UpdateFieldEditorContext context)
    {
        if (!viewModel.ToBeSaved ||
            string.IsNullOrEmpty(viewModel.UserAddressToSave) ||
            await _userService.GetCurrentFullUserAsync(_hca) is not { } user)
        {
            return;
        }

        await _userService.AlterUserSettingAsync(user, UserAddresses, contentItem =>
        {
            var part = contentItem[nameof(UserAddressesPart)].AsObject();

            if (part[viewModel.UserAddressToSave] is not JsonObject)
            {
                part[viewModel.UserAddressToSave] = JObject.FromObject(new AddressField());
            }

            part[viewModel.UserAddressToSave]![nameof(AddressField.Address)] = JObject.FromObject(viewModel.Address);
            return contentItem;
        });
    }
}
