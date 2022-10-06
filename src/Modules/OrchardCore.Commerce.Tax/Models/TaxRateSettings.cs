using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Tax.Models;

public class TaxRateSettings
{
    public string SourceStreetAddress1 { get; set; }
    public string SourceStreetAddress2 { get; set; }
    public string SourceCity { get; set; }
    public string SourceProvince { get; set; }
    public string SourcePostalCode { get; set; }
    public string SourceRegion { get; set; }

    public IList<TaxRateSetting> Rates { get; } = new List<TaxRateSetting>();

    public void CopyFrom(TaxRateSettings other)
    {
        SourceStreetAddress1 = other.SourceStreetAddress1;
        SourceStreetAddress2 = other.SourceStreetAddress2;
        SourceCity = other.SourceCity;
        SourceProvince = other.SourceProvince;
        SourcePostalCode = other.SourcePostalCode;
        SourceRegion = other.SourceRegion;

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
