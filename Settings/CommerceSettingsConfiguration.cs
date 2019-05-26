using System;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Money;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Settings
{
    public class CommerceSettingsConfiguration : IConfigureOptions<CommerceSettings>
    {
        private readonly ISiteService _site;

        public CommerceSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(CommerceSettings options)
        {
            var defaultCurrencySymbol = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<CommerceSettings>()
                .DefaultCurrency;

            options.DefaultCurrency =
                String.IsNullOrEmpty(defaultCurrencySymbol) ? Currency.Dollar.Symbol : defaultCurrencySymbol;
        }
    }
}
