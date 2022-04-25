using InternationalAddress;
using Microsoft.AspNetCore.Html;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
{
    private readonly IAddressFormatterProvider _addressFormatterProvider;

    public AddressFieldDisplayDriver(IAddressFormatterProvider addressFormatterProvider) => _addressFormatterProvider = addressFormatterProvider;

    public override IDisplayResult Edit(
        AddressField field,
        BuildFieldEditorContext context) =>
        Initialize<AddressFieldViewModel>(GetEditorShapeType(context), model => BuildViewModelAsync(model, field, context));

    private ValueTask BuildViewModelAsync(AddressFieldViewModel model, AddressField field, BuildFieldEditorContext context)
    {
        model.Address = field.Address;
        model.AddressHtml =
            new HtmlString(_addressFormatterProvider.Format(field.Address).Replace(System.Environment.NewLine, "<br/>"));
        model.Regions = Regions.All;
        foreach (var (key, value) in Regions.Provinces) model.Provinces[key] = value;
        model.ContentItem = field.ContentItem;
        model.AddressPart = field;
        model.PartFieldDefinition = context.PartFieldDefinition;

        return default;
    }
}
