using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions.Extensions;

public static class TableHeaders
{
    public static IList<LocalizedHtmlString> GetDefaultHeaders(IHtmlLocalizer htmlLocalizer, HeadersDisplayNamesOptions options) =>
    [
        htmlLocalizer[options.Product],
        htmlLocalizer[options.Quantity],
        htmlLocalizer[options.Price],
        htmlLocalizer[options.Action],
    ];
}
