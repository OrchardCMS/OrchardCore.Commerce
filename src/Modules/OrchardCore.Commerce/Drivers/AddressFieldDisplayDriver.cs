using Microsoft.AspNetCore.Html;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
{
    private readonly IAddressFormatterProvider _addressFormatterProvider;

    public AddressFieldDisplayDriver(IAddressFormatterProvider addressFormatterProvider) =>
        _addressFormatterProvider = addressFormatterProvider;

    public override IDisplayResult Edit(AddressField field, BuildFieldEditorContext context) =>
        Initialize<AddressFieldViewModel>(
            GetEditorShapeType(context),
            viewModel => PopulateViewModel(field, viewModel, context));

    public override async Task<IDisplayResult> UpdateAsync(AddressField field, IUpdateModel updater, UpdateFieldEditorContext context)
    {
        await updater.TryUpdateModelAsync(field, Prefix, addressField => addressField.Address);

        return await EditAsync(field, context);
    }

    private void PopulateViewModel(AddressField field, AddressFieldViewModel viewModel, BuildFieldEditorContext context)
    {
        viewModel.Address = field.Address;
        viewModel.AddressHtml =
            new HtmlString(_addressFormatterProvider.Format(field.Address).Replace(System.Environment.NewLine, "<br/>"));
        viewModel.Regions = Regions.All;
        foreach (var (key, value) in Regions.Provinces) viewModel.Provinces[key] = value;
        viewModel.ContentItem = field.ContentItem;
        viewModel.AddressPart = field;
        viewModel.PartFieldDefinition = context.PartFieldDefinition;
    }
}
