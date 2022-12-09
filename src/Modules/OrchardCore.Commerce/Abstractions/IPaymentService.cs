using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// When implemented handles the payment and creates an order.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Handles the payment and authentication, sends back the necessary data to the client./>.
    /// </summary>
    /// <returns>A new instance of <see cref="PaymentIntent"/>, or an existing one for the given
    /// <paramref name="paymentIntentId"/>.</returns>
    Task<PaymentIntent> InitializePaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Returns a <see cref="PaymentIntent"/> object for the given <paramref name="paymentIntentId"/>.
    /// </summary>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

    Task<PaymentIntent> CancelPaymentIntentWhenNeededAsync(string paymentIntentId = null, string reason = "abandoned");

    Task<PaymentIntent> CancelPaymentIntentWhenNeededAsync(PaymentIntent paymentIntent, string reason = "abandoned");

    /// <summary>
    /// Creates an order content item in the database, based on the <see cref="PaymentIntent"/> and on the current <see
    /// cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateOrderFromShoppingCartAsync(PaymentIntent paymentIntent);
}
