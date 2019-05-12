using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using static OrchardCore.Commerce.Money.Currency;

namespace OrchardCore.Commerce.Money
{
    public class ContentItemCurrencyProvider : ICurrencyProvider
    {
        private ISession _session;
        private static IEnumerable<ICurrency> _currencies;

        private static IDictionary<string, ICurrency> _currencyDictionary;

        public ContentItemCurrencyProvider(
            ISession session)
        {
            _session = session;

            var task = Task.Run(async () =>
            {
                var result = (await _session.Query<ContentItem, CurrencyPartIndex>().ListAsync());

                return result;
            });

            var contentItems = task.Result;

            var currencies = new List<ICurrency>();

            foreach (var contentItem in contentItems)
            {
                var currencyPart = contentItem.As<CurrencyPart>();

                currencies.Add(new Currency(currencyPart.Name, currencyPart.Symbol, currencyPart.IsoCode, "SE-sv", currencyPart.DecimalPlaces));
            }

            _currencies = currencies;

            // lookup via index in GetCurrency?
            _currencyDictionary = _currencies.ToDictionary(c => c.IsoCode);
        }

        public IEnumerable<ICurrency> Currencies => _currencies;

        public ICurrency GetCurrency(string isoSymbol) => _currencyDictionary[isoSymbol];
    }
}
