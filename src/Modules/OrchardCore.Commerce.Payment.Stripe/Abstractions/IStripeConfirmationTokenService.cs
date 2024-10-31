using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Service for managing Stripe confirmation tokens.
/// </summary>
public interface IStripeConfirmationTokenService
{
    /// <summary>
    /// Gets the Stripe confirmation token with an Id of <paramref name="confirmationTokenId"/>.
    /// </summary>
    /// <returns>The Stripe <see cref="ConfirmationToken"/>.</returns>
    Task<ConfirmationToken> GetConfirmationTokenAsync(string confirmationTokenId);
}
