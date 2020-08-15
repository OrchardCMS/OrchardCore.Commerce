using System.Threading.Tasks;
using InternationalAddress;
using Microsoft.AspNetCore.Html;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class AddressFieldDisplayDriver : ContentFieldDisplayDriver<AddressField>
    {
        private readonly IAddressFormatterProvider _addressFormatterProvider;

        public AddressFieldDisplayDriver(IAddressFormatterProvider addressFormatterProvider)
        {
            _addressFormatterProvider = addressFormatterProvider;
        }

        public override IDisplayResult Edit(AddressField addressField, BuildFieldEditorContext context)
        {
            return Initialize<AddressFieldViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, addressField, context));
        }

        private Task BuildViewModel(AddressFieldViewModel model, AddressField field, BuildFieldEditorContext context)
        {
            model.Address = field.Address;
            model.AddressHtml
                = new HtmlString(_addressFormatterProvider.Format(field.Address).Replace(System.Environment.NewLine, "<br/>"));
            model.Regions = Regions.All;
            model.Provinces = Regions.Provinces;
            model.ContentItem = field.ContentItem;
            model.AddressPart = field;
            model.PartFieldDefinition = context.PartFieldDefinition;

            return Task.CompletedTask;
        }
    }
}
