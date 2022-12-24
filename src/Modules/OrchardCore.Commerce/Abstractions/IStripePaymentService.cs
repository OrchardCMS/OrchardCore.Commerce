using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// When implemented handles the payment and creates an order.
/// </summary>
public interface IStripePaymentService
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

    /// <summary>
    /// Creates an order content item in the database, based on the <see cref="PaymentIntent"/> and on the current <see
    /// cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(PaymentIntent paymentIntent, IUpdateModelAccessor updateModelAccessor);

    /// <summary>
    /// Updates the corresponding order status to Order for the given <paramref name="paymentIntent"/>.
    /// </summary>
    Task UpdateOrderToOrderedAsync(PaymentIntent paymentIntent);

    /// <summary>
    /// Updates the corresponding order status to failed payment for the given <paramref name="paymentIntent"/>.
    /// </summary>
    Task UpdateOrderToPaymentFailedAsync(PaymentIntent paymentIntent);

    /// <summary>
    /// Return the saved <see cref="OrderPayment"/> for the given <paramref name="paymentIntentId"/>.
    /// </summary>
    Task<OrderPayment> GetOrderPaymentByPaymentIntentIdAsync(string paymentIntentId);
}
