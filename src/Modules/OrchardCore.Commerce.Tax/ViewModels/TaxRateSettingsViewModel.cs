using OrchardCore.Commerce.Tax.Models;
using System.Text.Json;

namespace OrchardCore.Commerce.Tax.ViewModels;

public class TaxRateSettingsViewModel : TaxRateSettings
{
    public string RatesJson { get; set; }

    public override void CopyFrom(TaxRateSettings other)
    {
        base.CopyFrom(other);
        RatesJson = JsonSerializer.Serialize(Rates, JOptions.CamelCase);
    }
}
