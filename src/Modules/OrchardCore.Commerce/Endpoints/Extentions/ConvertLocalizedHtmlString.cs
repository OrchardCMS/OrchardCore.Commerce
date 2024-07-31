using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Endpoints.Extensions;
public static class ConvertLocalizedHtmlString
{
    public static string ConvertLocalizedHtmlStringList(this IReadOnlyList<LocalizedHtmlString> errors) =>
        string.Join('\n', errors.Select(error => error.Value));
}
