using OrchardCore.Commerce.Indexes;
using OrchardCore.ContentManagement;
using System;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Handles looking up products by SKU.
/// </summary>
public class ProductPartContentAliasProvider : IContentHandleProvider
{
    private readonly ISession _session;

    public int Order => 57;

    public ProductPartContentAliasProvider(ISession session) => _session = session;

    public async Task<string> GetContentItemIdAsync(string handle)
    {
        if (handle.StartsWith("sku:", StringComparison.OrdinalIgnoreCase))
        {
            var sku = handle[4..].ToUpperInvariant();

            var productPartIndex = await _session
                .Query<ContentItem, ProductPartIndex>(index => index.Sku == sku)
                .FirstOrDefaultAsync();
            return productPartIndex?.ContentItemId;
        }

        return null;
    }
}
