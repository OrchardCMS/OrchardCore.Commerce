using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Models;

public class TaxRateSettings
{
    public IList<TaxRateSetting> Rates { get; } = new List<TaxRateSetting>();

    public MatchTaxRate MatchTaxRate { get; set; }

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

    public string TaxCode { get; set; }

    public decimal TaxRate { get; set; }
}

public enum MatchTaxRate
{
    Checked,
    Unchecked,
    Ignore,
}
