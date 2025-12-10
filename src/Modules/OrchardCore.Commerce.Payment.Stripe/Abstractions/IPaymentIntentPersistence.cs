using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Service that provides a way to retain the current payment intent Id from the current session.
/// </summary>
public interface IPaymentIntentPersistence
{
    /// <summary>
    /// Returns the payment intent Id stored in the current session.
    /// </summary>
    Task<string> RetrieveAsync(string shoppingCartId = null);

    /// <summary>
    /// Saves a payment intent Id to the session.
    /// </summary>
    Task StoreAsync(string paymentIntentId, string shoppingCartId = null);

    /// <summary>
    /// Removes the payment intent Id stored in the current session.
    /// </summary>
    Task RemoveAsync(string shoppingCartId = null);
}
