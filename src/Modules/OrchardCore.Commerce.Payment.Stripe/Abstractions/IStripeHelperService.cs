using Stripe;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Stripe helping services, needed so we can mock this part of Stripe also.
/// </summary>
public interface IStripeHelperService
{
    /// <summary>
    /// Parses a JSON string from a Stripe webhook into a <see cref="Event"/> object, while
    /// verifying the <a href="https://stripe.com/docs/webhooks/signatures">webhook's
    /// signature</a>.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="stripeSignatureHeader">
    /// The value of the <c>Stripe-Signature</c> header from the webhook request.
    /// </param>
    /// <param name="secret">The webhook endpoint's signing secret.</param>
    /// <param name="throwOnApiVersionMismatch">
    /// If <see langword="true"/> (default), the method will throw a <see cref="StripeException"/> if the
    /// API version of the event doesn't match Stripe.net's default API version (see
    /// <see cref="StripeConfiguration.ApiVersion"/>).
    /// </param>
    /// <returns>The deserialized <see cref="Event"/>.</returns>
    /// <exception cref="StripeException">
    /// Thrown if the signature verification fails for any reason, of if the API version of the
    /// event doesn't match Stripe.net's default API version.
    /// </exception>
    Event PrepareStripeEvent(string json, string stripeSignatureHeader, string secret, bool throwOnApiVersionMismatch);
}
