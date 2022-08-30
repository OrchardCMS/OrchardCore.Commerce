using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// When implemented handles the payment and creates an order.
/// </summary>
public interface ICardPaymentService
{
    /// <summary>
    /// Handles the payment and authentication, sends back the necessary data to the client./>.
    /// </summary>
    /// <returns>A new instance of <see cref="PaymentIntent"/> for the current payment.</returns>
    Task<PaymentIntent> CreatePaymentAsync(string paymentMethodId, string paymentIntentId);

    /// <summary>
    /// Creates an order content item in the database, based on the <see cref="PaymentIntent"/> and on the current <see
    /// cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateOrderFromShoppingCartAsync(PaymentIntent paymentIntent);
}
