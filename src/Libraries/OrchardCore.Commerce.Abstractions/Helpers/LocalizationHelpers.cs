using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions.Helpers;

public static class LocalizationHelpers
{
    public static IList<LocalizedHtmlString> GetLocalizedShoppingCartHeaders(IHtmlLocalizer<HeadersDisplayNamesOptions> htmlLocalizer, HeadersDisplayNamesOptions options) =>
    [
        htmlLocalizer[options.Product],
        htmlLocalizer[options.Quantity],
        htmlLocalizer[options.Price],
        htmlLocalizer[options.Action],
    ];
}
