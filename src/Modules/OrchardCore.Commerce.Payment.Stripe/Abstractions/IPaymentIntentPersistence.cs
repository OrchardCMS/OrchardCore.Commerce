#nullable enable
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Service that provides a way to retain the current payment intent Id from the current session.
/// </summary>
public interface IPaymentIntentPersistence
{
    /// <summary>
    /// Returns the payment intent information stored in the current session.
    /// </summary>
    Task<PaymentIntentPersistenceInfo?> RetrieveAsync(string? shoppingCartId, PaymentEnvironment environment);

    /// <summary>
    /// Saves a payment intent information to the session.
    /// </summary>
    Task StoreAsync(string? shoppingCartId, PaymentIntentPersistenceInfo info, PaymentEnvironment environment);

    /// <summary>
    /// Removes the payment intent information stored in the current session.
    /// </summary>
    Task RemoveAsync(string? shoppingCartId, PaymentEnvironment environment);
}
