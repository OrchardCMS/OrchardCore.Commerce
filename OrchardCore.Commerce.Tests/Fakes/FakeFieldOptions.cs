using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakeFieldOptions : IOptions<ContentOptions>
    {
        public FakeFieldOptions()
        {
            Value = new ContentOptions();
            Value.AddContentField<BooleanProductAttributeField>();
            Value.AddContentField<TextProductAttributeField>();
            Value.AddContentField<BooleanField>();
            Value.AddContentField<TextField>();
        }

        public ContentOptions Value { get; }
    }
}
