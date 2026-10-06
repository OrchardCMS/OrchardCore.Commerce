using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions.Helpers;

public static class LocalizationHelpers
{
    public static IList<LocalizedHtmlString> GetLocalizedShoppingCartHeaders(
        IHtmlLocalizer<HeadersDisplayNamesOptions> htmlLocalizer,
        HeadersDisplayNamesOptions options) =>
    [
        htmlLocalizer[options.Product],
        htmlLocalizer[options.Quantity],
        htmlLocalizer[options.Price],
        htmlLocalizer[options.Action],
    ];

    /// <summary>
    /// Gets the localized string for Net Price.
    /// </summary>
    public static LocalizedHtmlString GetLocalizedNetPriceName(
        IHtmlLocalizer<HeadersDisplayNamesOptions> htmlLocalizer,
        HeadersDisplayNamesOptions options) =>
        htmlLocalizer[options.NetPrice];

    /// <summary>
    /// Gets the localized string for Gross Price.
    /// </summary>
    public static LocalizedHtmlString GetLocalizedGrossPriceName(
        IHtmlLocalizer<HeadersDisplayNamesOptions> htmlLocalizer,
        HeadersDisplayNamesOptions options) =>
        htmlLocalizer[options.GrossPrice];

    /// <summary>
    /// Formats and localizes the <paramref name="priceName"/> and displays it next to the <paramref name="amount"/>
    /// (e.g., "Net Price: $10.00").
    /// </summary>
    public static LocalizedHtmlString FormatPriceWithLabel(
        IHtmlLocalizer<HeadersDisplayNamesOptions> htmlLocalizer,
        string priceName,
        object amount) =>
        htmlLocalizer["{0}: {1}", htmlLocalizer[priceName], amount];
}
