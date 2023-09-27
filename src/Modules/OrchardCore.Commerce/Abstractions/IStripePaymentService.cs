using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using Stripe;
using System.Collections.Generic;
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
    /// Calculates payment amount based on the given <paramref name="total"/>.
    /// </summary>
    long GetPaymentAmount(Amount total);

    /// <summary>
    /// Returns a <see cref="PaymentIntent"/> object based on the given <paramref name="total"/>.
    /// </summary>
    Task<PaymentIntent> CreatePaymentIntentAsync(long amountForPayment, Amount total);

    /// <summary>
    /// Creates an order content item in the database, based on the <see cref="PaymentIntent"/> and on the current <see
    /// cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(PaymentIntent paymentIntent, IUpdateModelAccessor updateModelAccessor);

    /// <summary>
    /// Updates the corresponding order status to <see cref="OrderStatuses.Ordered"/> for the given
    /// <paramref name="paymentIntent"/> or the provided <paramref name="orderItem"/>.
    /// </summary>
    Task UpdateOrderToOrderedAsync(PaymentIntent paymentIntent = null, ContentItem orderItem = null);

    /// <summary>
    /// Updates the corresponding order status to failed payment for the given <paramref name="paymentIntent"/>.
    /// </summary>
    Task UpdateOrderToPaymentFailedAsync(PaymentIntent paymentIntent);

    /// <summary>
    /// Return the saved <see cref="OrderPayment"/> for the given <paramref name="paymentIntentId"/>.
    /// </summary>
    Task<OrderPayment> GetOrderPaymentByPaymentIntentIdAsync(string paymentIntentId);

    /// <summary>
    /// Returns a <see cref="OrderLineItem"/> list from the given <paramref name="shoppingCart"/> items.
    /// </summary>
    Task<IEnumerable<OrderLineItem>> CreateOrderLineItemsAsync(ShoppingCart shoppingCart);
}
