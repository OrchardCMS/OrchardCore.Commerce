using OrchardCore.Commerce.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// When implemented handles the payment and creates an order.
/// </summary>
public interface ICardPaymentService
{
    /// <summary>
    /// Handles the payment and creates an order after a successful payment based on the current shopping cart content./>.
    /// </summary>
    /// <returns>A new instance of <see cref="CardPaymentViewModel"/> for the current payment.</returns>
    Task<CardPaymentReceiptViewModel> CreatePaymentAndOrderAsync(CardPaymentViewModel viewModel);
}
