using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.ViewModels;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class ShoppingCartCellViewModel
{
    public int LineIndex { get; set; }
    public int ColumnIndex { get; set; }
    public ShoppingCartLineViewModel Line { get; set; }
    public IList<(IProductAttributeValue Value, string Type, int Index)> ProductAttributes { get; } = [];

    public string Name => $"cart.lines[{LineIndex.ToTechnicalString()}]";
}
