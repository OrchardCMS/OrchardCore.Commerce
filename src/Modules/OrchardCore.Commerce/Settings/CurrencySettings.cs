namespace OrchardCore.Commerce.Settings;

/// <summary>
/// Basic settings for the commerce module.
/// </summary>
public class CurrencySettings
{
    /// <summary>
    /// Gets or sets the default currency ISO code.
    /// </summary>
    public string DefaultCurrency { get; set; }

    public string CurrentDisplayCurrency { get; set; }
}
