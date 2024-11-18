using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// When implemented handles the payment and creates an order.
/// </summary>
public interface IStripePaymentService
{
    /// <summary>
    /// Returns the public key of the Stripe account.
    /// </summary>
    Task<string> GetPublicKeyAsync();

    /// <summary>
    /// Handles the payment and authentication, sends back the necessary data to the client./>.
    /// </summary>
    Task<string> CreateClientSecretAsync(Amount total, ShoppingCartViewModel cart);

    /// <summary>
    /// Creates an order content item in the database, based on the stored <see cref="PaymentIntent"/> and on the
    /// current <see cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(
        IUpdateModelAccessor updateModelAccessor,
        string shoppingCartId,
        string paymentIntentId = null,
        OrderPart orderPart = null);

    /// <summary>
    /// Updates the corresponding order status to <see cref="OrderStatuses.Ordered"/> for the given
    /// <paramref name="paymentIntent"/>.
    /// </summary>
    Task UpdateOrderToOrderedAsync(PaymentIntent paymentIntent, string shoppingCartId);

    /// <summary>
    /// Updates the corresponding order status to failed payment for the given paymentIntentId.
    /// </summary>
    Task UpdateOrderToPaymentFailedAsync(string paymentIntentId);

    /// <summary>
    /// Return the saved <see cref="OrderPayment"/> for the given <paramref name="paymentIntentId"/>.
    /// </summary>
    Task<OrderPayment> GetOrderPaymentByPaymentIntentIdAsync(string paymentIntentId);

    /// <summary>
    /// Save the order payment for the given <paramref name="orderContentItemId"/> and <paramref name="paymentIntentId"/>.
    /// </summary>
    Task SaveOrderPaymentAsync(string orderContentItemId, string paymentIntentId);

    /// <summary>
    /// A shortcut method for updating the <paramref name="order"/> status to <see cref="OrderStatuses.Ordered"/>, doing
    /// final modifications and then redirecting to the success page.
    /// </summary>
    Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
        ContentItem order,
        PaymentIntent paymentIntent,
        string shoppingCartId
        );

    /// <summary>
    /// Get the confirmation parameters for Stripe.
    /// </summary>
    /// <param name="returnUrl">The url for the middleware of Stripe.</param>
    Task<PaymentIntentConfirmOptions> GetStripeConfirmParametersAsync(
        string returnUrl,
        ContentItem order = null);

    /// <summary>
    /// Confirm the result of Stripe payment.
    /// </summary>
    /// <param name="paymentIntentId">The Payment Intent Id from Stripe.</param>
    /// <param name="shoppingCartId">The Shopping Cart Id of this application.</param>
    /// <param name="needToJudgeIntentStorage">To judge if this method should use the storage of <see cref="PaymentIntentPersistence"/>.</param>
    /// <returns>The status of payment operation.</returns>
    Task<PaymentOperationStatusViewModel> PaymentConfirmationAsync(
        string paymentIntentId,
        string shoppingCartId,
        bool needToJudgeIntentStorage = true);
}
