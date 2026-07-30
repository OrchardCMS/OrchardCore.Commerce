using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

public class LocalizationDuplicateSkuResolver : IDuplicateSkuResolver
{
    public IReadOnlyList<ContentItem> UpdateDuplicatesList(ContentItem current, IReadOnlyList<ContentItem> otherProducts) =>
        current.GetMaybe<LocalizationPart>()?.LocalizationSet is { } currentLocalizationSet
            ? otherProducts
                .WhereNot(other => other.GetMaybe<LocalizationPart>()?.LocalizationSet == currentLocalizationSet)
                .ToList()
            : otherProducts;
}
