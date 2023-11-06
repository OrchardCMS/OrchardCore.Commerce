using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ContentFields.Events;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System;
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
            var part = contentItem.GetJObject(nameof(UserAddressesPart));

            if (part[viewModel.UserAddressToSave] is not JObject)
            {
                part[viewModel.UserAddressToSave] = JObject.FromObject(new AddressField());
            }

            if (part.GetJObject(viewModel.UserAddressToSave) is not { } userAddressToSave)
            {
                throw new InvalidOperationException(
                    $"The property {viewModel.UserAddressToSave} is missing from {nameof(UserAddressesPart)}.");
            }

            userAddressToSave[nameof(AddressField.Address)] = JToken.FromObject(viewModel.Address);
            return contentItem;
        });
    }
}
