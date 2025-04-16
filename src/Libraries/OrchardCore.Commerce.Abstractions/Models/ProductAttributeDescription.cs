using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Abstractions.Models;

public class ProductAttributeDescription
{
    public string Name { get; }
    public string PartName { get; }
    public ProductAttributeField Field { get; }
    public ProductAttributeFieldSettings Settings { get; }

    public ProductAttributeDescription(
        string name,
        string partName,
        ProductAttributeField field,
        ProductAttributeFieldSettings settings)
    {
        Name = name;
        PartName = partName;
        Field = field;
        Settings = settings;
    }
}
