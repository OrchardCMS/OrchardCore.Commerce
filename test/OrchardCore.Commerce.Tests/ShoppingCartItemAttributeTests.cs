using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using System.Collections.Generic;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class ShoppingCartItemAttributeTests
{
    private readonly HashSet<IProductAttributeValue> _attrSet1Parsed = new()
    {
        new TextProductAttributeValue("ProductPart1.Size", "small"),
        new TextProductAttributeValue("ProductPart1.Color", "green"),
    };

    [Fact]
    public void GenerateVariantKeyFromShoppingCartItemAttributes()
    {
        var item = new ShoppingCartItem(5, "foo", _attrSet1Parsed);
        var attribs = new HashSet<string>
        {
            "ProductPart1.Size",
            "ProductPart1.Color",
        };

        var variantKey = item.GetVariantKeyFromAttributes(attribs);
        Assert.Equal("GREEN-SMALL", variantKey);
    }
}
