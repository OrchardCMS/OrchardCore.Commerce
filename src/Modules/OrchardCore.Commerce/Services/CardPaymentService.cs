using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ViewModels;
using Stripe;
using System;

namespace OrchardCore.Commerce.Services;
public class CardPaymentService : ICardPaymentService
{
    private const int PaymentAmountPence = 100;
    private const string PaymentCurrency = "usd";
    private const string PaymentDescription = "Orchard Commerce Test Stripe Card Payment";
    // "false" prevents the card being charged immediately!
    private const bool CaptureCardPayment = true;

    private readonly ChargeService _chargeService;

    public CardPaymentService() =>
        _chargeService = new ChargeService();

    public CardPaymentReceiptViewModel Create(CardPaymentViewModel viewModel)
    {
        var paymentTransactionId = Guid.NewGuid().ToString();

        var chargeCreateOptions = new ChargeCreateOptions
        {
            TransferGroup = paymentTransactionId,
            Amount = PaymentAmountPence,
            Currency = PaymentCurrency,
            Description = PaymentDescription,
            Source = viewModel.Token,
            Capture = CaptureCardPayment,
            ReceiptEmail = viewModel.Email,
        };

        var charge = _chargeService.Create(chargeCreateOptions);

        return ToPaymentReceipt(charge);
    }

    private static CardPaymentReceiptViewModel ToPaymentReceipt(Charge charge)
    {
        var cardPaymentReceiptViewModel = new CardPaymentReceiptViewModel
        {
            Amount = charge.Amount,
            Currency = charge.Currency,
            Description = charge.Description,
            Status = charge.Status,
            Created = charge.Created,
            BalanceTransactionId = charge.BalanceTransactionId,
            Id = charge.Id,
            SourceId = charge.Source.Id,
        };

        return cardPaymentReceiptViewModel;
    }
}
