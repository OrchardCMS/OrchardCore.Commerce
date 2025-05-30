using OrchardCore.Commerce.MoneyDataType;
using System.Linq;
using static OrchardCore.Commerce.Payment.Stripe.Constants.CurrencyCollectionConstants;

namespace OrchardCore.Commerce.Payment.Stripe.Helpers;

public static class AmountHelpers
{
    public static long GetPaymentAmount(Amount total)
    {
        var rounding = ZeroDecimalCurrencies.Select(code => (Code: code, KeepDigits: 0, RoundTens: 0))
            .Concat(SpecialCases.Select(code => (Code: code, KeepDigits: 2, RoundTens: 2)))
            .ToDictionary(item => item.Code, item => (item.KeepDigits, item.RoundTens));

        return total.GetFixedPointAmount(rounding);
    }
}
