using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceBookService
    {
        Task<IContent> GetStandardPriceBook();
        Task SetStandardPriceBook(PriceBookPart priceBookPart);

        Task<IEnumerable<IContent>> GetAllPriceBooks();
        Task<IEnumerable<IContent>> GetPriceBookEntriesByPriceBook(string priceBookContentItemId);
        Task<IEnumerable<IContent>> GetPriceBookEntriesByProduct(string productContentItemId);
        Task RemovePriceBookEntriesByProduct(string productContentItemId);

        Task<IEnumerable<PriceBookPrice>> GetPriceBookPrices(IEnumerable<IContent> priceBooks, ProductPart productPart);

        Task<IEnumerable<IContent>> GetPriceBooksFromRules();

        Task<string> GeneratePriceBookEntryTitle(PriceBookEntryPart model, string productTitle);
    }
}
