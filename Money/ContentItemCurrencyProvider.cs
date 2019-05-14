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

        private static IDictionary<string, ICurrency> _currencyDictionary;

        public ContentItemCurrencyProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _currencyDictionary = new Dictionary<string, ICurrency>();
        }

        public IEnumerable<ICurrency> Currencies
        {
            get
            {
                Initialize();

                return from a in _currencyDictionary
                       select a.Value;
            }
            private set { }
        }

        public ICurrency GetCurrency(string isoSymbol)
        {
            if (_currencyDictionary.Count() == 0)
                Initialize();

            return _currencyDictionary[isoSymbol];
        }

        private void Initialize()
        {
            if (_currencyDictionary.Count() == 0)
            {
                var contentItems = GetSession().Query<ContentItem, CurrencyPartIndex>().ListAsync().Result;

                var currencies = new List<ICurrency>();

                foreach (var contentItem in contentItems)
                {
                    var currencyPart = contentItem.As<CurrencyPart>();

                    _currencyDictionary.Add(currencyPart.IsoCode, new Currency(currencyPart.Name, currencyPart.Symbol, currencyPart.IsoCode, currencyPart.Culture, currencyPart.DecimalPlaces));
                }
            }
        }

        public Task AddOrUpdateAsync(ICurrency currency)
        {
            _currencyDictionary[currency.IsoCode] = currency;

            return Task.CompletedTask;
        }

        public Task RemoveAsync(ICurrency currency)
        {
            _currencyDictionary.Remove(currency.IsoCode);

            return Task.CompletedTask;
        }

        private YesSql.ISession GetSession()
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext.RequestServices.GetService<YesSql.ISession>();
        }
    }
}
