using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;
using System.Linq;
using SystemEnvironment = System.Environment;

namespace OrchardCore.Commerce.Endpoints.Extensions;

public static class ConvertLocalizedHtmlString
{
    public static string ConvertLocalizedHtmlStringList(this IReadOnlyList<LocalizedHtmlString> errors) =>
        string.Join(SystemEnvironment.NewLine, errors.Select(error => error.Html()));
}
