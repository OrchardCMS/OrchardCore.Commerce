using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Drivers;

public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
{
    private readonly IAddressFormatterProvider _addressFormatterProvider;
    private readonly IHttpContextAccessor _hca;
    private readonly IUserService _userService;
    private readonly IRegionService _regionService;

    public AddressFieldDisplayDriver(
        IAddressFormatterProvider addressFormatterProvider,
        IHttpContextAccessor hca,
        IUserService userService,
        IRegionService regionService)
    {
        _addressFormatterProvider = addressFormatterProvider;
        _hca = hca;
        _userService = userService;
        _regionService = regionService;
    }

    public override IDisplayResult Display(AddressField field, BuildFieldDisplayContext fieldDisplayContext) =>
        Initialize<AddressFieldViewModel>(
                GetDisplayShapeType(fieldDisplayContext),
                viewModel =>
                {
                    PopulateViewModel(field, viewModel, fieldDisplayContext.PartFieldDefinition);

                    viewModel.AddressHtml = new HtmlString(
                        _addressFormatterProvider
                            .Format(field.Address)
                            .Replace(System.Environment.NewLine, "<br/>"));
                })
            .Location("Detail", "Content")
            .Location("Summary", "Content");

    public override IDisplayResult Edit(AddressField field, BuildFieldEditorContext context) =>
        Initialize<AddressFieldViewModel>(
            GetEditorShapeType(context),
            async viewModel =>
            {
                viewModel.UserAddressToSave = field.UserAddressToSave;

                viewModel.Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions();
                viewModel.Provinces.AddRange(Regions.Provinces);
                PopulateViewModel(field, viewModel, context.PartFieldDefinition);
            });

    public override async Task<IDisplayResult> UpdateAsync(AddressField field, IUpdateModel updater, UpdateFieldEditorContext context)
    {
        var viewModel = new AddressFieldViewModel();

        if (!await updater.TryUpdateModelAsync(viewModel, Prefix)) return await EditAsync(field, context);

        field.Address = viewModel.Address;

        if (viewModel.ToBeSaved &&
            !string.IsNullOrEmpty(viewModel.UserAddressToSave) &&
            await _userService.GetCurrentFullUserAsync(_hca) is { } user)
        {
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

        return await EditAsync(field, context);
    }

    private static void PopulateViewModel(
        AddressField field,
        AddressFieldViewModel viewModel,
        ContentPartFieldDefinition contentPartFieldDefinition)
    {
        viewModel.Address = field.Address;
        viewModel.ContentItem = field.ContentItem;
        viewModel.AddressPart = field;
        viewModel.PartFieldDefinition = contentPartFieldDefinition;
    }
}
