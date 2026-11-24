using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A product variants prices based on predefined attributes.
/// </summary>
public class PriceVariantsPart : ContentPart
{
    public Amount? this[string key] => Variants.TryGetValue(key, out var value) ? value : null;

    public IDictionary<string, Amount> Variants { get; } = new Dictionary<string, Amount>(StringComparer.OrdinalIgnoreCase);
}
