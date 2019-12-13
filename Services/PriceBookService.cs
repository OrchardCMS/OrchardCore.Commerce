using Money;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.CompiledQueries;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using YesSql;
using Money.Abstractions;

namespace OrchardCore.Commerce.Services
{
    public class PriceBookService : IPriceBookService
    {
        private readonly ISession _session;
        private readonly IEnumerable<IPriceBookRuleProvider> _priceBookRuleProviders;
        private readonly IContentManager _contentManager;
        private readonly ICurrencyProvider _currencyProvider;
                
        public PriceBookService(ISession session,
            IEnumerable<IPriceBookRuleProvider> priceBookRuleProviders,
            IContentManager contentManager,
            ICurrencyProvider currencyProvider)
        {
            _session = session;
            _priceBookRuleProviders = priceBookRuleProviders;
            _contentManager = contentManager;
            _currencyProvider = currencyProvider;
        }

        public async Task<IContent> GetStandardPriceBook()
        {
            var allPriceBooks = await GetAllPriceBooks();
            return allPriceBooks
                .Where(apb => apb.ContentItem.As<PriceBookPart>() != null
                    && apb.ContentItem.As<PriceBookPart>().StandardPriceBook)
                .FirstOrDefault();
        }

        public async Task SetStandardPriceBook(PriceBookPart priceBookPart)
        {
            // Remove standard price book if it is exists and isn't current price book
            var standardPriceBook = await GetStandardPriceBook();
            var standardPriceBookPart = standardPriceBook?.ContentItem.As<PriceBookPart>();
            if (standardPriceBookPart != null && standardPriceBookPart != priceBookPart)
            {
                // Remove standard price book
                standardPriceBook.ContentItem.Alter<PriceBookPart>(p => p.StandardPriceBook = false);
                await _contentManager.UpdateAsync(standardPriceBook.ContentItem);
            }

            // Set current price book to standard price book
            priceBookPart.StandardPriceBook = true;
        }
        
        
        public async Task<IEnumerable<IContent>> GetAllPriceBooks()
        {
            return await _session
                .ExecuteQuery(new ContentItemsByContentTypePublished(CommerceConstants.ContentTypes.PriceBook))
                .ListAsync();
        }
        
        public async Task<IEnumerable<IContent>> GetPriceBookEntriesByPriceBook(string priceBookContentItemId)
        {
            var priceBookEntryPartIndexes = await _session
                .ExecuteQuery(new PriceBookEntryByPriceBook(priceBookContentItemId))
                .ListAsync();

            var priceBookEntryContentItemIds = priceBookEntryPartIndexes
                .Select(i => i.ContentItemId);
            return await _contentManager.GetAsync(priceBookEntryContentItemIds);
        }

        public async Task<IEnumerable<IContent>> GetPriceBookEntriesByProduct(string productContentItemId)
        {
            var priceBookEntryPartIndexes = await _session
                .ExecuteQuery(new PriceBookEntryByProduct(productContentItemId))
                .ListAsync();

            var priceBookEntryContentItemIds = priceBookEntryPartIndexes
                .Select(i => i.ContentItemId);
            return await _contentManager.GetAsync(priceBookEntryContentItemIds);
        }


        public async Task<IEnumerable<PriceBookPrice>> GetPriceBookPrices(IEnumerable<IContent> priceBooks, ProductPart productPart)
        {
            var priceBookPrices = new List<PriceBookPrice>();
            var activePriceBookContentItemIds = priceBooks.Select(p => p.ContentItem.ContentItemId);
            var standardPriceBook = await GetStandardPriceBook();
            var standardPriceBookContentItemId = standardPriceBook.ContentItem.ContentItemId;

            var priceBookEntryPartIndexes = await _session
                .ExecuteQuery(new PriceBookEntryByProduct(productPart.ContentItem.ContentItemId))
                .ListAsync();

            var activePriceBookEntryPartIndexes = priceBookEntryPartIndexes
                .Where(i => activePriceBookContentItemIds.Contains(i.PriceBookContentItemId));

            var priceBookEntries = await _contentManager.GetAsync(activePriceBookEntryPartIndexes.Select(a => a.ContentItemId));

            foreach (var activePriceBookEntryPartIndex in activePriceBookEntryPartIndexes)
            {
                var priceBook = priceBooks
                    .Where(p => p.ContentItem.ContentItemId == activePriceBookEntryPartIndex.PriceBookContentItemId)
                    .FirstOrDefault();

                var priceBookEntry = priceBookEntries
                    .Where(p => p.ContentItemId == activePriceBookEntryPartIndex.ContentItemId)
                    .FirstOrDefault();

                Amount? price = null;

                if (activePriceBookEntryPartIndex.UseStandardPrice)
                {
                    // Need to retrieve the standard price (if available)
                    var standardPriceBookEntryPartIndex = priceBookEntryPartIndexes
                        .Where(i => i.PriceBookContentItemId == standardPriceBookContentItemId)
                        .FirstOrDefault();

                    if (standardPriceBookEntryPartIndex != null)
                    {
                        var currency = _currencyProvider.GetCurrency(standardPriceBookEntryPartIndex.AmountCurrencyIsoCode);
                        price = new Amount(standardPriceBookEntryPartIndex.AmountValue.Value, currency);
                    }
                }
                else
                {
                    var currency = _currencyProvider.GetCurrency(activePriceBookEntryPartIndex.AmountCurrencyIsoCode);
                    price = new Amount(activePriceBookEntryPartIndex.AmountValue.Value, currency);
                }

                if (price.HasValue)
                {
                    priceBookPrices.Add(new PriceBookPrice()
                    {
                        PriceBook = priceBook,
                        PriceBookEntry = priceBookEntry,
                        Price = price.Value
                    });
                }
            }

            return priceBookPrices;
        }

        
        private async Task<IEnumerable<PriceBookRule>> GetApplicablePriceBooksRules()
        {
            var priceBooksRules = new List<PriceBookRule>();
            foreach (var priceBookRuleProvider in _priceBookRuleProviders)
            {
                priceBooksRules.AddRange(
                    (await priceBookRuleProvider.GetPriceBookRules())
                    .Where(p => p.Applies())
                );
            }
            return priceBooksRules;
        }

        public async Task<IEnumerable<IContent>> GetPriceBooksFromRules()
        {
            var priceBookRules = await GetApplicablePriceBooksRules();

            // At this point, we are not taking "Weight" into consideration, returning all matching price books
            // TODO: A future setting could say to take the "heaviest" weight and leave the rest
            return await _contentManager.GetAsync(priceBookRules.Select(p => p.PriceBookContentItemId));
        }

    }
}
