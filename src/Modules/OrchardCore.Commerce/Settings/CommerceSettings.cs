using Money;

namespace OrchardCore.Commerce.Settings;

/// <summary>
/// Basic settings for the commerce module.
/// </summary>
public class CommerceSettings
{
    /// <summary>
    /// Gets or sets the default currency ISO code. Their initial value is "USD".
    /// </summary>
    public string DefaultCurrency { get; set; } = Currency.UsDollar.CurrencyIsoCode;

    public string CurrentDisplayCurrency { get; set; } = Currency.UsDollar.CurrencyIsoCode;
}
