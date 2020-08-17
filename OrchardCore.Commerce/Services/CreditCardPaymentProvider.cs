using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    public class CreditCardPaymentProvider : IPaymentProvider
    {
        private const string Last4 = nameof(Last4);
        private const string ExpirationMonth = nameof(ExpirationMonth);
        private const string ExpirationYear = nameof(ExpirationYear);

        private readonly IStringLocalizer S;

        public CreditCardPaymentProvider(IStringLocalizer<CreditCardPaymentProvider> localizer)
        {
            S = localizer;
        }

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
            => kind == CreditCardPayment.CreditCardKind ?
            new CreditCardPayment
            {
                TransactionId = transactionId,
                Amount = amount,
                ChargeText = S["Card **** **** **** {0} expiring {1}/{2}.", data[Last4], data[ExpirationMonth], data[ExpirationYear]].ToString(),
                Last4 = data[Last4],
                ExpirationMonth = int.TryParse(data[ExpirationMonth], out int expMonth) && expMonth >= 1 && expMonth <=12 ? expMonth : 0,
                ExpirationYear = int.TryParse(data[ExpirationYear], out int expYear) && expYear >= 0 ? expYear : 0
            } : null;
    }
}
