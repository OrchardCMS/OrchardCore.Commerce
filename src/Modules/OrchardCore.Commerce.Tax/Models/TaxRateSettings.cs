using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Models;

public class TaxRateSettings
{
    public string SourceStreetAddress1 { get; set; }
    public string SourceStreetAddress2 { get; set; }
    public string SourceCity { get; set; }
    public string SourceProvince { get; set; }
    public string SourcePostalCode { get; set; }
    public string SourceRegion { get; set; }

    public IEnumerable<TaxRateSetting> Type { get; set; }
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
}
