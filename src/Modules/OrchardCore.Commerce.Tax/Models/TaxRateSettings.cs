using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

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

    [JsonInclude]
    public bool IsValid =>
        TaxRate != 0 && !(
            string.IsNullOrEmpty(DestinationStreetAddress1) &&
            string.IsNullOrEmpty(DestinationStreetAddress2) &&
            string.IsNullOrEmpty(DestinationCity) &&
            string.IsNullOrEmpty(DestinationProvince) &&
            string.IsNullOrEmpty(DestinationPostalCode) &&
            string.IsNullOrEmpty(DestinationRegion) &&
            string.IsNullOrEmpty(VatNumber) &&
            string.IsNullOrEmpty(TaxCode));

    public TaxRateSetting() { }

    public TaxRateSetting(IDictionary<string, string> rawRate)
    {
        DestinationStreetAddress1 = rawRate.GetMaybe(nameof(DestinationStreetAddress1));
        DestinationStreetAddress2 = rawRate.GetMaybe(nameof(DestinationStreetAddress2));
        DestinationCity = rawRate.GetMaybe(nameof(DestinationCity));
        DestinationProvince = rawRate.GetMaybe(nameof(DestinationProvince));
        DestinationPostalCode = rawRate.GetMaybe(nameof(DestinationPostalCode));
        DestinationRegion = rawRate.GetMaybe(nameof(DestinationRegion));
        VatNumber = rawRate.GetMaybe(nameof(VatNumber));
        TaxCode = rawRate.GetMaybe(nameof(TaxCode));

        IsCorporation = ParseMatchTaxRates(rawRate.GetMaybe(nameof(IsCorporation)));

        TaxRate = decimal.TryParse(rawRate.GetMaybe(nameof(TaxRate)), CultureInfo.InvariantCulture, out var taxRate)
            ? taxRate
            : 0;
    }

    private static MatchTaxRates ParseMatchTaxRates(string value) =>
        value switch
        {
            nameof(MatchTaxRates.Ignored) => MatchTaxRates.Ignored,
            nameof(MatchTaxRates.Checked) => MatchTaxRates.Checked,
            nameof(MatchTaxRates.Unchecked) => MatchTaxRates.Unchecked,
            { } when int.TryParse(value, CultureInfo.InvariantCulture, out var number) => (MatchTaxRates)number,
            _ => default,
        };
}

public enum MatchTaxRates
{
    Ignored,
    Checked,
    Unchecked,
}
