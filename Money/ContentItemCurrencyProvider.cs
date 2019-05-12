using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Commerce.Money
{
    public class ContentItemCurrencyProvider : ICurrencyProvider
    {
        private IServiceProvider _serviceProvider;
        private static IEnumerable<ICurrency> _currencies;

        private static IDictionary<string, ICurrency> _currencyDictionary;

        public ContentItemCurrencyProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<ICurrency> Currencies
        {
            get { return Initialize(); }
            private set { }
        }

        public ICurrency GetCurrency(string isoSymbol)
        {
            if (_currencies == null)
                Initialize();

            return _currencyDictionary[isoSymbol];
        }

        private IEnumerable<ICurrency> Initialize()
        {
            if (_currencies == null || _currencies.Count() == 0)
            {
                var contentItems = GetSession().Query<ContentItem, CurrencyPartIndex>().ListAsync().Result;

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

            return _currencies;
        }

        private YesSql.ISession GetSession()
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext.RequestServices.GetService<YesSql.ISession>();
        }
    }
}
