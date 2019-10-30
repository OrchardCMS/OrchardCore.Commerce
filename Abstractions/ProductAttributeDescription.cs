using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Abstractions
{
    public class ProductAttributeDescription
    {
        public ProductAttributeDescription(string name, string partName, ProductAttributeField field, ProductAttributeFieldSettings settings)
        {
            Name = name;
            PartName = partName;
            Field = field;
            Settings = settings;
        }

        public string Name { get; }
        public string PartName { get; }
        public ProductAttributeField Field { get; }
        public ProductAttributeFieldSettings Settings { get; }
    }
}
