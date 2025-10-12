using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Services;

public interface ITaxRateSettingsHeaderContainer
{
    public IReadOnlyDictionary<string, string> HeaderLabels { get; }
}
