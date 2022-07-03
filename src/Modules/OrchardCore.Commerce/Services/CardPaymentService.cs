using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.ViewModels;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;
public class CardPaymentService : ICardPaymentService
{
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IPriceService _priceService;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly ChargeService _chargeService;

    public CardPaymentService(
        IShoppingCartPersistence shoppingCartPersistence,
        IPriceService priceService,
        IPriceSelectionStrategy priceSelectionStrategy)
    {
        _chargeService = new ChargeService();
        _shoppingCartPersistence = shoppingCartPersistence;
        _priceService = priceService;
        _priceSelectionStrategy = priceSelectionStrategy;
    }

    public async Task<CardPaymentReceiptViewModel> CreateAsync(CardPaymentViewModel viewModel)
    {
        var totals = await (await _shoppingCartPersistence.RetrieveAsync())
            .CalculateTotalsAsync(_priceService, _priceSelectionStrategy);

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies.
        var defaultTotal = totals.FirstOrDefault();

        var chargeCreateOptions = new ChargeCreateOptions
        {
            Amount = (long?)(defaultTotal.Value * 100),
            Currency = defaultTotal.Currency.CurrencyIsoCode,
            Description = "Orchard Commerce Test Stripe Card Payment",
            Source = viewModel.Token,
            Capture = true,
            // If shipping is implemented, it needs to be added here too.
            // Shipping =
            ReceiptEmail = viewModel.Email,
        };

        Charge finalCharge;

        try
        {
            var charge = _chargeService.Create(chargeCreateOptions);
            finalCharge = charge;
        }
        catch (StripeException e)
        {
            return ToPaymentReceipt(charge: null, e);
        }

        return ToPaymentReceipt(finalCharge);
    }

    private static CardPaymentReceiptViewModel ToPaymentReceipt(Charge charge, StripeException excpetion = null) =>
        charge != null
        ? new CardPaymentReceiptViewModel
        {
            Amount = charge.Amount,
            Currency = charge.Currency,
            Description = charge.Description,
            Status = charge.Status,
            Created = charge.Created,
            BalanceTransactionId = charge.BalanceTransactionId,
            Id = charge.Id,
            SourceId = charge.Source.Id,
            Exception = excpetion,
        }
        : new CardPaymentReceiptViewModel
        {
            Exception = excpetion,
        };
}
