using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Constants;

public static class CurrencyCollectionConstants
{
    public static readonly IEnumerable<string> SpecialCases = ["ISK", "HUF", "TWD", "UGX"];

    // Note that https://docs.stripe.com/currencies#zero-decimal contains UGX on the list as well, however this is a
    // mistake because https://docs.stripe.com/currencies#special-cases explicitly states that UGX is a special case
    // (see above) where it's effectively zero-decimal but uses the two-decimal format for backwards compatibility.
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
