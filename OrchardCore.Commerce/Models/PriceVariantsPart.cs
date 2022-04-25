using Money;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A product variants prices based on predefined attributes.
/// </summary>
public class PriceVariantsPart : ContentPart
{
    public IDictionary<string, Amount> Variants { get; } = new Dictionary<string, Amount>();
}
