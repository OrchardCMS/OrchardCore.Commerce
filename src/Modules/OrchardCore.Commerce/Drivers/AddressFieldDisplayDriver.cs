using Microsoft.AspNetCore.Html;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
{
    private readonly IAddressFormatterProvider _addressFormatterProvider;

    public AddressFieldDisplayDriver(IAddressFormatterProvider addressFormatterProvider) =>
        _addressFormatterProvider = addressFormatterProvider;

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
                viewModel.Regions = Regions.All;
                viewModel.Provinces.AddRange(Regions.Provinces);
                PopulateViewModel(field, viewModel, context.PartFieldDefinition);
            });

    public override async Task<IDisplayResult> UpdateAsync(AddressField field, IUpdateModel updater, UpdateFieldEditorContext context)
    {
        await updater.TryUpdateModelAsync(field, Prefix, addressField => addressField.Address);

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
