using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using YesSql;

namespace OrchardCore.Commerce.Services
{
    public class ProductService : IProductService
    {
        private ISession _session;

        public ProductService(ISession session)
        {
            _session = session;
        }

        public async Task<ProductPart> GetProduct(string sku)
        {
            var contentItemId = (await _session.QueryIndex<ProductPartIndex>(x => x.Sku == sku).FirstOrDefaultAsync())?.Id;
            return contentItemId.HasValue ? await _session.GetAsync<ProductPart>(contentItemId.Value) : null;
        }

        public async Task<IEnumerable<ProductPart>> GetProducts(params string[] skus)
        {
            var contentItemIds = (await _session
                .QueryIndex<ProductPartIndex>(x => skus.Contains(x.Sku))
                .ListAsync())
                .Select(idx => idx.Id)
                .Distinct()
                .ToArray();
            return await _session.GetAsync<ProductPart>(contentItemIds);
        }
    }
}
