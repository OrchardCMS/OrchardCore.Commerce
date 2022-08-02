using Microsoft.Extensions.Options;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Settings;

public class CommerceSettingsCurrencySelector : ICurrencySelector
{
    private readonly CommerceSettings _options;

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

    public CommerceSettingsCurrencySelector(IOptions<CommerceSettings> options) => _options = options.Value;
}
