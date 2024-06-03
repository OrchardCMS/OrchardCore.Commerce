using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Endpoints.Extensions;
public static class ConvertLocalizedHtmlString
{
    public static string ConvertLocalizedHtmlStringList(this IReadOnlyList<LocalizedHtmlString> errors)
    {
        var list = (from item in errors
                    let strItem = item.Value
                    select strItem).ToList();
        string strError = string.Join('\n', list);
        return strError;
    }
}
