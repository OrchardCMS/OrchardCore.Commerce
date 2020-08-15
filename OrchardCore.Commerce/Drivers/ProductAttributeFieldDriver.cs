using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public abstract class ProductAttributeFieldDriver<TField, TFieldSettings> : ContentFieldDisplayDriver<TField>
        where TField : ProductAttributeField, new()
        where TFieldSettings : ProductAttributeFieldSettings, new()
    {
        public ProductAttributeFieldDriver(
            IStringLocalizer<ProductAttributeFieldDriver<TField, TFieldSettings>> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Edit(TField field, BuildFieldEditorContext context)
        {
            return Initialize<EditProductAttributeFieldViewModel<TField, TFieldSettings>>(
                GetEditorShapeType(context), model =>
                {
                    var settings = new TFieldSettings();
                    context.PartFieldDefinition.PopulateSettings(settings);
                    model.Field = field;
                    model.Settings = settings;
                    model.Part = context.ContentPart;
                    model.PartFieldDefinition = context.PartFieldDefinition;
                });
        }
    }

    public class BooleanProductAttributeFieldDriver
        : ProductAttributeFieldDriver<BooleanProductAttributeField, BooleanProductAttributeFieldSettings>
    {
        public BooleanProductAttributeFieldDriver(
            IStringLocalizer<ProductAttributeFieldDriver<BooleanProductAttributeField, BooleanProductAttributeFieldSettings>> localizer)
            : base(localizer) { }
    }

    public class NumericProductAttributeFieldDriver
        : ProductAttributeFieldDriver<NumericProductAttributeField, NumericProductAttributeFieldSettings>
    {
        public NumericProductAttributeFieldDriver(
            IStringLocalizer<ProductAttributeFieldDriver<NumericProductAttributeField, NumericProductAttributeFieldSettings>> localizer)
            : base(localizer) { }
    }

    public class TextProductAttributeFieldDriver
        : ProductAttributeFieldDriver<TextProductAttributeField, TextProductAttributeFieldSettings>
    {
        public TextProductAttributeFieldDriver(
            IStringLocalizer<ProductAttributeFieldDriver<TextProductAttributeField, TextProductAttributeFieldSettings>> localizer)
            : base(localizer) { }
    }
}
