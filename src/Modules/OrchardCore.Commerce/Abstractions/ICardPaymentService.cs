using OrchardCore.Commerce.Models;
using Stripe;
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
    /// <returns>A new instance of <see cref="ConfirmPaymentRequest"/> for the current payment.</returns>
    Task<PaymentIntent> CreatePaymentAsync(ConfirmPaymentRequest request);

    /// <summary>
    /// Creates an order content item in the database, based on the <see cref="PaymentIntent"/> and on the current <see
    /// cref="ShoppingCart"/> content.
    /// </summary>
    Task CreateOrderFromShoppingCartAsync(PaymentIntent paymentIntent);
}
