using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using System;
using System.Collections.Generic;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class ShoppingCartItemAttributeTests
{
    private readonly HashSet<IProductAttributeValue> _attrSet1Parsed =
    [
        new TextProductAttributeValue("ProductPart1.Size", "small"),
        new TextProductAttributeValue("ProductPart1.Color", "green"),
    ];

    [Fact]
    [Obsolete(
        $"{nameof(ShoppingCartItem.GetVariantKeyFromAttributes)} is superseded by " +
        $"{nameof(IProductAttributeService.GetProductKey)}, which is validated by the price variant UI tests.")]
    public void GenerateVariantKeyFromShoppingCartItemAttributes()
    {
        var item = new ShoppingCartItem(5, "foo", _attrSet1Parsed);
        var attributes = new HashSet<string>
        {
            "ProductPart1.Size",
            "ProductPart1.Color",
        };

        var variantKey = item.GetVariantKeyFromAttributes(attributes);
        Assert.Equal("GREEN-SMALL", variantKey);
    }
}
