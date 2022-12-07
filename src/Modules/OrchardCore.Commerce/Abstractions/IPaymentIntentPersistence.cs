namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service that provides a way to retain shopping cart information.
/// </summary>
public interface IPaymentIntentPersistence
{
    /// <summary>
    /// Returns the payment intent id stored in current session.
    /// </summary>
    string Retrieve();

    /// <summary>
    /// Saves a payment intent Id.
    /// </summary>
    void Store(string paymentIntentId);
}
