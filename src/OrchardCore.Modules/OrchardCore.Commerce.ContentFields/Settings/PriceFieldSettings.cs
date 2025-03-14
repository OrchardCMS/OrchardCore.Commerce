namespace OrchardCore.Commerce.ContentFields.Settings;

public class PriceFieldSettings
{
    public string Hint { get; set; }
    public string Label { get; set; }
    public bool Required { get; set; }

    public CurrencySelectionMode CurrencySelectionMode { get; set; }
    public string SpecificCurrencyIsoCode { get; set; }
}
