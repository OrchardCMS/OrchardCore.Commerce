using Stripe;
using System;

namespace OrchardCore.Commerce.ViewModels;

public class CardPaymentReceiptViewModel
{
    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public DateTime Created { get; set; }

    public string BalanceTransactionId { get; set; }

    public string Id { get; set; }

    public StripeException Exception { get; set; }
}
