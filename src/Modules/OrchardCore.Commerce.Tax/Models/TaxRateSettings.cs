using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Models;

public class TaxRateSettings
{
    public IList<TaxRateSetting> Rates { get; } = [];

    public void CopyFrom(TaxRateSettings other)
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
}

public enum MatchTaxRates
{
    Ignored,
    Checked,
    Unchecked,
}
