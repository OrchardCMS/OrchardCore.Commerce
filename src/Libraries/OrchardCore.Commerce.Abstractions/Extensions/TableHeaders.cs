using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions.Extensions;

public static class TableHeaders
{
    public static IList<LocalizedHtmlString> GetDefaultHeaders(IHtmlLocalizer htmlLocalizer) =>
    [
        htmlLocalizer["Product"],
        htmlLocalizer["Quantity"],
        htmlLocalizer["Price"],
        htmlLocalizer["Action"],
    ];
}
