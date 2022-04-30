using Microsoft.Extensions.Localization;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Commerce.Services;

public class CreditCardPaymentProvider : IPaymentProvider
{
    private const string Last4 = nameof(Last4);
    private const string ExpirationMonth = nameof(ExpirationMonth);
    private const string ExpirationYear = nameof(ExpirationYear);

    private readonly IStringLocalizer T;

    public CreditCardPaymentProvider(IStringLocalizer<CreditCardPaymentProvider> localizer) => T = localizer;

    public void AddData(IPayment charge, IDictionary<string, string> data)
    {
        if (charge is CreditCardPayment creditCardCharge)
        {
            data[Last4] = creditCardCharge.Last4;
            data[ExpirationMonth] = creditCardCharge.ExpirationMonth.ToString(CultureInfo.InvariantCulture);
            data[ExpirationYear] = creditCardCharge.ExpirationYear.ToString(CultureInfo.InvariantCulture);
        }
    }

    public IPayment CreateCharge(string kind, string transactionId, Amount amount, IDictionary<string, string> data)
    {
        if (kind != CreditCardPayment.CreditCardKind) return null;

        var last4 = data[Last4];
        var expirationMonth = data[ExpirationMonth];
        var expirationYear = data[ExpirationYear];

        return new CreditCardPayment
        {
            TransactionId = transactionId,
            Amount = amount,
            ChargeText = T["Card **** **** **** {0} expiring {1}/{2}.", last4, expirationMonth, expirationYear].ToString(),
            Last4 = last4,
            ExpirationMonth = int.TryParse(expirationMonth, out var expMonth) && expMonth is >= 1 and <= 12 ? expMonth : 0,
            ExpirationYear = int.TryParse(expirationYear, out var expYear) && expYear >= 0 ? expYear : 0,
        };
    }
}
