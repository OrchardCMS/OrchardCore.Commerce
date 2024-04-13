using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Entities;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class ContentLocalizationProductService : ProductService
{
    private readonly ISiteService _siteService;

    public ContentLocalizationProductService(
        ISiteService siteService,
        ISession session,
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IPredefinedValuesProductAttributeService predefinedValuesService,
        Lazy<IShoppingCartSerializer> shoppingCartSerializer)
        : base(session, contentManager, contentDefinitionManager, predefinedValuesService, shoppingCartSerializer) =>
            _siteService = siteService;

    public override async Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus)
    {
        var skuList = skus.AsList();
        var products = (await base.GetProductsAsync(skuList)).AsList();

        // Shortcut if there are no duplicate localized products.
        if (skuList.Count == products.Count ||
            (await _siteService.GetSiteSettingsAsync()).As<LocalizationSettings>() is not { } localizationSettings)
        {
            return products;
        }

        var result = new List<ProductPart>();
        var priority = GetPrioritySupportedCultures(localizationSettings);

        foreach (var group in products.GroupBy(part => part.Sku))
        {
            var parts = group.ToList();

            if (parts.Count == 1)
            {
                result.Add(parts[0]);
                continue;
            }

            result.Add(parts
                .OrderByDescending(part => priority.IndexOf(part.ContentItem.As<LocalizationPart>()?.Culture))
                .First());
        }

        return result;
    }

    /// <summary>
    /// Returns a list of available cultures in order of ascending priority. This means the highest priority culture is
    /// the last, so this list can be used with <see cref="List{T}.IndexOf(T)"/> to sort by descending order and anything
    /// not on this list will be correctly sorted to the back due to the -1 index.
    /// </summary>
    private static IList<string> GetPrioritySupportedCultures(LocalizationSettings settings)
    {
        var list = settings
            .SupportedCultures
            .OrderBy(culture => culture == settings.DefaultCulture ? 1 : 0)
            .ThenBy(culture => culture == "en-US" ? 1 : 0)
            .ThenBy(culture => culture)
            .ToList();

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].PartitionEnd("-").Left is { } parent && !list.Contains(parent))
            {
                list.Insert(i + 1, parent);
            }
        }

        return list;
    }
}
