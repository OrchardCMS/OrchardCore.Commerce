using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.ContentFields.Events;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
{
    private static readonly Dictionary<string, Func<Address, string>> RequiredFields = new()
    {
        [nameof(Address.Name)] = address => address.Name,
        [nameof(Address.StreetAddress1)] = address => address.StreetAddress1,
        [nameof(Address.City)] = address => address.City,
    };

    private readonly IEnumerable<IAddressUpdater> _addressUpdaters;
    private readonly IEnumerable<IAddressFieldEvents> _addressFieldEvents;
    private readonly IAddressFormatterProvider _addressFormatterProvider;
    private readonly IHttpContextAccessor _hca;
    private readonly IRegionService _regionService;
    private readonly IStringLocalizer<AddressFieldDisplayDriver> T;

    public AddressFieldDisplayDriver(
        IEnumerable<IAddressUpdater> addressUpdaters,
        IEnumerable<IAddressFieldEvents> addressFieldEvents,
        IAddressFormatterProvider addressFormatterProvider,
        IHttpContextAccessor hca,
        IRegionService regionService,
        IStringLocalizer<AddressFieldDisplayDriver> stringLocalizer)
    {
        _addressUpdaters = addressUpdaters;
        _addressFieldEvents = addressFieldEvents;
        _addressFormatterProvider = addressFormatterProvider;
        _hca = hca;
        _regionService = regionService;
        T = stringLocalizer;
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
            .Location(CommonContentDisplayTypes.Detail, CommonLocationNames.Content)
            .Location(CommonContentDisplayTypes.Summary, CommonLocationNames.Content);

    public override IDisplayResult Edit(AddressField field, BuildFieldEditorContext context) =>
        Initialize<AddressFieldViewModel>(
            GetEditorShapeType(context),
            async viewModel =>
            {
                viewModel.UserAddressToSave = field.UserAddressToSave;

                viewModel.Regions = (await _regionService.GetAvailableRegionsAsync()).CreateSelectListOptions();
                viewModel.Provinces.AddRange(await _regionService.GetAllProvincesAsync());
                PopulateViewModel(field, viewModel, context.PartFieldDefinition);
            });

    public override async Task<IDisplayResult> UpdateAsync(AddressField field, UpdateFieldEditorContext context)
    {
        if (await TryUpdateModelAsync(context) is { } viewModel)
        {
            field.Address = viewModel.Address;
            await _addressFieldEvents.AwaitEachAsync(handler => handler.UpdatingAsync(viewModel, field, context.Updater, context));
        }

        return await EditAsync(field, context);
    }

    private async Task<AddressFieldViewModel> TryUpdateModelAsync(UpdateFieldEditorContext context)
    {
        var viewModel = await context.CreateModelAsync<AddressFieldViewModel>(Prefix);
        if (viewModel.Address is not { } address) return null;

        await _addressUpdaters.AwaitEachAsync(addressUpdater => addressUpdater.UpdateAsync(address));

        // We have to detect if we are in the user editor in the admin dashboard, because then it's okay to save even if
        // the normally required fields are left empty.
        if (_hca.HttpContext?.Request.RouteValues["area"]?.ToString() == "OrchardCore.Users") return viewModel;

        var missingFields = RequiredFields
            .Where(pair => string.IsNullOrWhiteSpace(pair.Value(address)))
            .Select(pair => pair.Key)
            .ToList();

        foreach (var key in missingFields)
        {
            // This doesn't need to be too complex as it's just a fallback from the client-side validation.
            context.AddModelError(key, T["A value is required for {0}.", key]);
        }

        return missingFields.Count != 0 ? null : viewModel;
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
