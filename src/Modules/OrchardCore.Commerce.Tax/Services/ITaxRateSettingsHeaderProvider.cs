using OrchardCore.Commerce.Tax.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Services;

/// <summary>
/// Provides the header texts for the custom tax rate editor in the admin dashboard.
/// </summary>
public interface ITaxRateSettingsHeaderProvider
{
    /// <summary>
    /// Gets the header texts. The key is the property name in <see cref="TaxRateSetting"/>, the value is the localized
    /// string.
    /// </summary>
    public IReadOnlyDictionary<string, string> HeaderLabels { get; }
}
