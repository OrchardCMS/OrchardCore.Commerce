using System.Threading.Tasks;
using OrchardCore.Commerce.Indexes;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// Handles looking up products by SKU.
    /// </summary>
    public class ProductPartContentAliasProvider : IContentAliasProvider
    {
        private readonly ISession _session;

        public ProductPartContentAliasProvider(ISession session)
        {
            _session = session;
        }

        public int Order => 57;
        
        public async Task<string> GetContentItemIdAsync(string alias)
        {
            if (alias.StartsWith("sku:", System.StringComparison.OrdinalIgnoreCase))
            {
                var sku = alias.Substring(4).ToLowerInvariant();

                var productPartIndex = await _session
                    .Query<ContentItem, ProductPartIndex>(x => x.Sku == sku)
                    .FirstOrDefaultAsync();
                return productPartIndex?.ContentItemId;
            }

            return null;
        }
    }
}
