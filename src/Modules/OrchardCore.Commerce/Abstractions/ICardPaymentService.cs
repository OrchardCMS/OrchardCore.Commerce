using OrchardCore.Commerce.ViewModels;

namespace OrchardCore.Commerce.Abstractions;
public interface ICardPaymentService
{
    CardPaymentReceiptViewModel Create(CardPaymentViewModel viewModel);
}
