using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions;

public interface IPredefinedValuesProductAttributeFieldSettings
{
    /// <summary>
    /// Gets a value indicating whether values should be restricted to the set of predefined values
    /// </summary>
    public bool RestrictToPredefinedValues { get; }

    /// <summary>
    /// Gets the set of suggested or allowed values
    /// </summary>
    public IEnumerable<object> PredefinedValues { get; }
}
