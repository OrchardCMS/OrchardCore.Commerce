using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.Extensions;
public static class ConvertLocalizedHtmlString
{
    public static string ConvertLocalizedHtmlStringList(this IReadOnlyList<LocalizedHtmlString> errors)
    {
        var list = new List<string>();
        foreach (var item in errors)
        {
            var strItem = item.Value;
            list.Add(strItem);
        }

        string strError = string.Join('\n', list);
        return strError;
    }
}
