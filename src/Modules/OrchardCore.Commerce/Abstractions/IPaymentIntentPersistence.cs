using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service that provides a way to retain shopping cart information.
/// </summary>
public interface IPaymentIntentPersistence
{
    /// <summary>
    /// Returns a <see cref="PaymentIntent"/> identified by <paramref name="paymentIntentId"/>.
    /// </summary>
    string Retrieve();

    /// <summary>
    /// Saves a payment intent Id.
    /// </summary>
    void Store(string paymentIntentId);
}
