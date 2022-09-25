using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Drivers;

public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
{
    private readonly IAddressFormatterProvider _addressFormatterProvider;
    private readonly IContentManager _contentManager;
    private readonly IHttpContextAccessor _hca;
    private readonly ISession _session;
    private readonly UserManager<IUser> _userManager;

    public AddressFieldDisplayDriver(
        IAddressFormatterProvider addressFormatterProvider,
        IContentManager contentManager,
        IHttpContextAccessor hca,
        ISession session,
        UserManager<IUser> userManager)
    {
        _addressFormatterProvider = addressFormatterProvider;
        _contentManager = contentManager;
        _hca = hca;
        _session = session;
        _userManager = userManager;
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
            viewModel =>
            {
                viewModel.UserAddressToSave = field.UserAddressToSave;

                viewModel.Regions = Regions.All;
                viewModel.Provinces.AddRange(Regions.Provinces);
                PopulateViewModel(field, viewModel, context.PartFieldDefinition);
            });

    public override async Task<IDisplayResult> UpdateAsync(AddressField field, IUpdateModel updater, UpdateFieldEditorContext context)
    {
        var viewModel = new AddressFieldViewModel();

        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            field.Address = viewModel.Address;

            if (viewModel.ToBeSaved &&
                !string.IsNullOrEmpty(viewModel.UserAddressToSave) &&
                _hca.HttpContext?.User.Identity?.IsAuthenticated == true &&
                await _userManager.GetUserAsync(_hca.HttpContext.User) is User user)
            {
                if (GetJObject(user.Properties, UserAddresses, nameof(UserAddressesPart)) is not { } part)
                {
                    user.Properties[UserAddresses] = JObject.FromObject(await _contentManager.NewAsync(UserAddresses));
                    part = GetJObject(user.Properties, UserAddresses, nameof(UserAddressesPart));
                }

                if (part[viewModel.UserAddressToSave] is not JObject)
                {
                    part[viewModel.UserAddressToSave] = JObject.FromObject(new AddressField());
                }

                if (GetJObject(part, viewModel.UserAddressToSave) is not { } userAddressToSave)
                {
                    throw new InvalidOperationException(
                        $"The property {viewModel.UserAddressToSave} is missing from {nameof(UserAddressesPart)}.");
                }

                userAddressToSave[nameof(AddressField.Address)] = JToken.FromObject(viewModel.Address);
                _session.Save(user);
            }
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

    [SuppressMessage(
        "Major Code Smell",
        "S1168:Empty arrays and collections should be returned instead of null",
        Justification = "An empty JObject makes no sense here.")]
    private static JObject GetJObject(JObject node, params string[] properties)
    {
        foreach (var property in properties)
        {
            if (!node.TryGetValue(property, out var child) || child is not JObject childObject) return null;
            node = childObject;
        }

        return node;
    }
}
