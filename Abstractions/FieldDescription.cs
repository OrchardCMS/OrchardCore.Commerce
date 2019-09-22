using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public class FieldDescription
    {
        public FieldDescription(string name, string partName, ContentField field)
        {
            Name = name;
            PartName = partName;
            Field = field;
        }

        public string Name { get; }
        public string PartName { get; }
        public ContentField Field { get; }
    }
}
