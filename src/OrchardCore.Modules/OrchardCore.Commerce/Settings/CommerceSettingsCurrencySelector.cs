using Microsoft.Extensions.Options;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Settings
{
    public class CommerceSettingsCurrencySelector : ICurrencySelector
    {
        private readonly CommerceSettings _options;

        public CommerceSettingsCurrencySelector(IOptions<CommerceSettings> options)
        {
            _options = options.Value;
        }
        public ICurrency CurrentDisplayCurrency => Currency.FromISOCode(_options.CurrentDisplayCurrency);
    }
}
