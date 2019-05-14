using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Commerce.Handlers
{
    public class CommerceContentHandler : ContentHandlerBase
    {
        private ICurrencyProvider _currencyProvider;

        public CommerceContentHandler(ICurrencyProvider currencyProvider)
        {
            _currencyProvider = currencyProvider;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            var contentItem = context.ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentNullException("contentItem");
            }

            if (contentItem.Id == 0)
            {
                return Task.CompletedTask;
            }

            var currencyPart = contentItem.As<CurrencyPart>();

            if (currencyPart == null)
            {
                return Task.CompletedTask;
            }

            return _currencyProvider.AddOrUpdateAsync(new Currency(currencyPart.Name, currencyPart.Symbol, currencyPart.IsoCode, currencyPart.Culture, currencyPart.DecimalPlaces));
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            var contentItem = context.ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentNullException("contentItem");
            }

            if (contentItem.Id == 0)
            {
                return Task.CompletedTask;
            }

            var currencyPart = contentItem.As<CurrencyPart>();

            if (currencyPart == null)
            {
                return Task.CompletedTask;
            }

            return _currencyProvider.RemoveAsync(new Currency(currencyPart.Name, currencyPart.Symbol, currencyPart.IsoCode, currencyPart.Culture, currencyPart.DecimalPlaces));
        }
    }
}
