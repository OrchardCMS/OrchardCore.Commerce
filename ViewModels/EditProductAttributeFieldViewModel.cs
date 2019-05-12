using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.ViewModels
{
    public class EditProductAttributeFieldViewModel<TField, TFieldSettings>
        where TField : ProductAttributeField
        where TFieldSettings : ProductAttributeFieldSettings
    {
        public TField Field { get; set; }
        public TFieldSettings Settings { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
