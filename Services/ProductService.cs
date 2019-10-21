using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Commerce.Services
{
    public class ProductService : IProductService
    {
        private ISession _session;
        private IContentManager _contentManager;

        public ProductService(
            ISession session,
            IContentManager contentManager)
        {
            _session = session;
            _contentManager = contentManager;
        }

        public async Task<ProductPart> GetProduct(string sku)
        {
            var contentItemId = (await _session.QueryIndex<ProductPartIndex>(x => x.Sku == sku).FirstOrDefaultAsync())?.ContentItemId;
            return contentItemId is null ? null : (await _contentManager.GetAsync(contentItemId)).As<ProductPart>();
        }

        public async Task<IEnumerable<ProductPart>> GetProducts(IEnumerable<string> skus)
        {
            var contentItemIds = (await _session
                .QueryIndex<ProductPartIndex>(x => x.Sku.IsIn(skus))
                .ListAsync())
                .Select(idx => idx.ContentItemId)
                .Distinct()
                .ToArray();
            return (await _contentManager.GetAsync(contentItemIds))
                .Select(item => item.As<ProductPart>());
        }
    }
}
