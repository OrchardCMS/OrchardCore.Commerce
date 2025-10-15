using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Tax.Models;

public class TaxRateSettings
{
    public IList<TaxRateSetting> Rates { get; } = [];

    public virtual void CopyFrom(TaxRateSettings other)
    {
        Rates.Clear();
        Rates.AddRange(other.Rates);
    }
}

public class TaxRateSetting
{
    public string DestinationStreetAddress1 { get; set; }
    public string DestinationStreetAddress2 { get; set; }
    public string DestinationCity { get; set; }
    public string DestinationProvince { get; set; }
    public string DestinationPostalCode { get; set; }
    public string DestinationRegion { get; set; }
    public string VatNumber { get; set; }
    public string TaxCode { get; set; }

    public MatchTaxRates IsCorporation { get; set; }

    public decimal TaxRate { get; set; }

    [JsonIgnore]
    public bool IsEmpty =>
        TaxRate == 0 &&
        IsCorporation == MatchTaxRates.Ignored &&
        string.IsNullOrEmpty(DestinationStreetAddress1) &&
        string.IsNullOrEmpty(DestinationStreetAddress2) &&
        string.IsNullOrEmpty(DestinationCity) &&
        string.IsNullOrEmpty(DestinationProvince) &&
        string.IsNullOrEmpty(DestinationPostalCode) &&
        string.IsNullOrEmpty(DestinationRegion) &&
        string.IsNullOrEmpty(VatNumber) &&
        string.IsNullOrEmpty(TaxCode);
}

public enum MatchTaxRates
{
    Ignored,
    Checked,
    Unchecked,
}
