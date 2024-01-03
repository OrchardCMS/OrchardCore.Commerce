using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Constants;
public static class CurrencyCollectionConstants
{
    public static readonly IEnumerable<string> SpecialCases = new List<string> { "HUF", "TWD", "UGX" };
    public static readonly IEnumerable<string> ZeroDecimalCurrencies = new List<string>
    {
        "BIF",
        "CLP",
        "DJF",
        "GNF",
        "JPY",
        "KMF",
        "KRW",
        "MGA",
        "PYG",
        "RWF",
        "UGX",
        "VND",
        "VUV",
        "XAF",
        "XOF",
        "XPF",
    };
}
