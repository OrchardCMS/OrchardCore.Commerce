using Money;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Models;

public class CreditCardPayment : IPayment
{
    public static readonly string CreditCardKind = "Credit Card";

    public string Kind => CreditCardKind;

    public string TransactionId { get; set; }

    public string ChargeText { get; set; }

    public Amount Amount { get; set; }
}
