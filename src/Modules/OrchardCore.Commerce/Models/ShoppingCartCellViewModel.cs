using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ViewModels;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class ShoppingCartCellViewModel
{
    public int LineIndex { get; set; }
    public int ColumnIndex { get; set; }
    public ShoppingCartLineViewModel Line { get; set; }
    public IList<(IProductAttributeValue Value, string Type, int Index)> ProductAttributes { get; } =
        new List<(IProductAttributeValue Value, string Type, int Index)>();

    public string Name => $"cart.lines[{LineIndex.ToTechnicalString()}]";
}
