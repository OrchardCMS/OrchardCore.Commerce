using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

public class LocalizationDuplicateSkuResolver : IDuplicateSkuResolver
{
    public IList<ContentItem> UpdateDuplicatesList(ContentItem current, IList<ContentItem> otherProducts) =>
        current.GetOrCreate<LocalizationPart>()?.LocalizationSet is { } currentLocalizationSet
            ? otherProducts
                .WhereNot(other => other.GetOrCreate<LocalizationPart>()?.LocalizationSet == currentLocalizationSet)
                .ToList()
            : otherProducts;
}
