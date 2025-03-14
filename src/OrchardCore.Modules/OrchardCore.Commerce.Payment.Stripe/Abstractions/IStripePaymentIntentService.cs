using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Service for managing Stripe Payment Intents.
/// </summary>
public interface IStripePaymentIntentService
{
    /// <summary>
    /// Gets a PaymentIntent by its Stripe Id.
    /// </summary>
    /// <returns>Stripe <see cref="PaymentIntent"/> model.</returns>
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Gets the PaymentIntent by its Stripe Id if it is <see cref="PaymentIntentStatuses.Succeeded"/> or
    /// <see cref="PaymentIntentStatuses.Processing"/>. Otherwise, updates it with the provided
    /// <paramref name="defaultTotal"/>.
    /// </summary>
    /// <returns>Updated or original Stripe <see cref="PaymentIntent"/> model.</returns>
    Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        Amount defaultTotal);

    /// <summary>
    /// Creates a PaymentIntent with the provided <paramref name="total"/>. And adds description and other values to
    /// the payment intent. Check the implementation for more details.
    /// </summary>
    /// <returns>Created Stripe <see cref="PaymentIntent"/>.</returns>
    Task<PaymentIntent> CreatePaymentIntentAsync(Amount total);

    /// <summary>
    /// Creates a PaymentIntent with the provided <paramref name="options"/>.
    /// </summary>
    /// <returns>Created Stripe <see cref="PaymentIntent"/> model.</returns>
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);
}
