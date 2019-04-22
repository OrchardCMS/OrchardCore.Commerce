using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using static OrchardCore.Commerce.Money.Currency;

namespace OrchardCore.Commerce.Money
{
    public class ContentItemCurrencyProvider : ICurrencyProvider
    {
        private IContentManager _contentManager;
        private ISession _session;
        private static IEnumerable<ICurrency> _currencies;

        private static IDictionary<string, ICurrency> _currencyDictionary;

        public ContentItemCurrencyProvider(
            IContentManager contentManager,
            ISession session)
        {
            _contentManager = contentManager;
            _session = session;

            var task = Task.Run(async () =>
            {
                var result = (await _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentType == "Currency")
                    .ListAsync()).ToList();

                //var currencies = await _contentManager.GetAsync(contentItems);

                return result;
            });

            var contentItems = task.Result;

            var currencies = new List<ICurrency>();

            foreach (var contentItem in contentItems)
            {
                var currencyContentItem = _contentManager.LoadAsync(contentItem).Result;

                //currencies.Add(new Currency(
                //    currencyContentItem.DisplayText,
                //    currencyContentItem.Get("Symbol"),
                //    currencyContentItem.Get("IsoCode"),
                //    currencyContentItem.elem
                //    null
                //    )
                //);
                
            }

            //_currencies = currencies;

            // load from contentitems
            _currencies = new List<ICurrency>() { Dollar, Euro, Yen, PoundSterling, AustralianDollar,
                CanadianDollar, SwissFranc, Renminbi, SwedishKrona, Currency.BitCoin};


            // lookup via index in GetCurrency?
            _currencyDictionary = _currencies.ToDictionary(c => c.IsoCode);
        }

        public IEnumerable<ICurrency> Currencies => _currencies;

        public ICurrency GetCurrency(string isoSymbol) => _currencyDictionary[isoSymbol];
    }
}
