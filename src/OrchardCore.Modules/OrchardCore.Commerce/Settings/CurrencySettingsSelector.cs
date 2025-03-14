using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;

namespace OrchardCore.Commerce.Settings;

public class CurrencySettingsSelector : ICurrencySelector
{
    private readonly CurrencySettings _options;

    public ICurrency CurrentDisplayCurrency
    {
        get
        {
            var defaultIsoCode = _options?.CurrentDisplayCurrency;
            return string.IsNullOrEmpty(defaultIsoCode)
                ? Currency.UsDollar
                : Currency.FromIsoCode(defaultIsoCode) ?? Currency.UsDollar;
        }
    }

    public CurrencySettingsSelector(IOptions<CurrencySettings> options) => _options = options.Value;
}
