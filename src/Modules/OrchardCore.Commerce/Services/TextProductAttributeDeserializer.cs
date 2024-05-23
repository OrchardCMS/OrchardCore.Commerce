using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.ProductAttributeValues;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OrchardCore.Commerce.Services;

public class TextProductAttributeDeserializer : IProductAttributeDeserializer
{
    public string AttributeTypeName => nameof(TextProductAttributeValue);

    public IProductAttributeValue Deserialize(string attributeName, JsonObject attribute) =>
        new TextProductAttributeValue(attributeName, attribute["value"].GetValue<IEnumerable<string>>());
}

public class BooleanProductAttributeDeserializer : IProductAttributeDeserializer
{
    public string AttributeTypeName => nameof(BooleanProductAttributeValue);

    public IProductAttributeValue Deserialize(string attributeName, JsonObject attribute) =>
        new BooleanProductAttributeValue(attributeName, attribute["value"].GetValue<bool>());
}

public class NumericProductAttributeDeserializer : IProductAttributeDeserializer
{
    public string AttributeTypeName => nameof(NumericProductAttributeValue);

    public IProductAttributeValue Deserialize(string attributeName, JsonObject attribute) =>
        new NumericProductAttributeValue(attributeName, attribute["value"].GetValue<decimal>());
}
