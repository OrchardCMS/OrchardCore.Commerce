using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;
public class TestController : Controller
{
    private readonly ILogger _logger;
    private const string GenericEuCultureName = "en-EU";
    internal static IDictionary<string, ICurrency> CurrencyTable { get; private set; }


    public TestController(ILogger<TestController> logger)
    {

        _logger = logger;
    }

    public IActionResult Index()
    {
        static bool IsValid(CultureInfo cultureInfo) =>
            cultureInfo.Name.Contains('-') &&
            !cultureInfo.IsNeutralCulture &&
            !cultureInfo.EnglishName.StartsWith("Unknown Locale", StringComparison.Ordinal) &&
            !cultureInfo.EnglishName.StartsWith("Invariant Language", StringComparison.Ordinal);

        static int RankCultureByExpectedRelevance(CultureInfo cultureInfo)
        {
            var parts = cultureInfo.Name.Split('-');

            // Prioritize when the language and country ISO codes match, e.g. hu-HU, no-NO, etc. This doesn't help with
            // cultures like sv-SE, ja-SP or fa-IR.
            if (parts.Length == 2 && parts[0].ToUpperInvariant() == parts[1]) return 0;

            // English is usually a safe choice on the Internet.
            if (cultureInfo.TwoLetterISOLanguageName == "en") return 1;

            return int.MaxValue; // Fallback, the rest are sorted alphabetically.
        }

        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(IsValid).ToList();
        cultures.Add(new CultureInfo(GenericEuCultureName));
        _logger.LogError("Cultures count {0} | {1}", cultures.Count, cultures.Select(culture => culture.Name).Join(", "));

        CurrencyTable = cultures
            .GroupBy(culture => culture.Name.Split('-').Last())
            .Select(group => group
                .OrderBy(RankCultureByExpectedRelevance)
                .ThenBy(culture => culture.EnglishName)
                .First())
            .Select(culture => new Currency(culture, logger: _logger))
            .Where(currency => currency.CurrencyIsoCode != "EUR" || currency.Culture.Name == GenericEuCultureName)
            .Cast<ICurrency>()
            .Distinct(new CurrencyEqualityComparer())
            .ToDictionary(currency => currency.CurrencyIsoCode, currency => currency);

        CurrencyTable.Add("BTC", new Currency("BitCoin", "BitCoin", "₿", "BTC", 8));
        CurrencyTable.Add("---", Currency.UnspecifiedCurrency);

        return View();
    }

    private sealed class CurrencyEqualityComparer : IEqualityComparer<ICurrency>
    {
        public bool Equals(ICurrency left, ICurrency right) =>
            (left is null && right is null) ||
            StringComparer.OrdinalIgnoreCase.Equals(left?.CurrencyIsoCode, right?.CurrencyIsoCode);

        public int GetHashCode(ICurrency obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.CurrencyIsoCode);
    }
}
