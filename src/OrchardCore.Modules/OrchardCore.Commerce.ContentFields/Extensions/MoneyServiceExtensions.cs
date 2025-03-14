using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.ContentFields.Settings;
using System;
using System.Linq;

namespace OrchardCore.Commerce.MoneyDataType.Abstractions;

public static class MoneyServiceExtensions
{
    /// <summary>
    /// Creates a <see cref="SelectList"/> from the applicable array of <see cref="ICurrency"/> objects. If <paramref
    /// name="localizer"/> is not <see langword="null"/> then the display text will be long form including the
    /// currency's localized name.
    /// </summary>
    public static SelectList GetSelectableCurrencies(
        this IMoneyService moneyService,
        CurrencySelectionMode mode,
        string specificCurrencyIsoCode = null,
        IStringLocalizer localizer = null,
        string currentValue = null)
    {
        var currencies = mode switch
        {
            CurrencySelectionMode.DefaultCurrency => [moneyService.DefaultCurrency],
            CurrencySelectionMode.SpecificCurrency => [moneyService.GetCurrency(specificCurrencyIsoCode)],
            CurrencySelectionMode.AllCurrencies => moneyService.Currencies,
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };

        var items = currencies
            .Where(currency => !string.IsNullOrEmpty(currency.EnglishName))
            .OrderBy(currency => currency.CurrencyIsoCode)
            .Select(currency => new SelectListItem(
                localizer == null
                    ? currency.CurrencyIsoCode
                    : $"{currency.CurrencyIsoCode} {currency.Symbol} - {localizer[currency.EnglishName]}",
                currency.CurrencyIsoCode))
            .ToList();

        return new(
            items,
            nameof(SelectListItem.Value),
            nameof(SelectListItem.Text),
            currentValue ?? items.FirstOrDefault()?.Value);
    }
}
