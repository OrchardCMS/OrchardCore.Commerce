using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Constants;
public static class CurrencyCollectionConstants
{
    public static readonly IEnumerable<string> SpecialCases = ["HUF", "TWD", "UGX"];
    public static readonly IEnumerable<string> ZeroDecimalCurrencies =
    [
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
        "VND",
        "VUV",
        "XAF",
        "XOF",
        "XPF",
    ];
}
