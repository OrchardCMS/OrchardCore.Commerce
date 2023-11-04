namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service that provides a way to retain the current payment intent Id from the current session.
/// </summary>
public interface IPaymentIntentPersistence
{
    /// <summary>
    /// Returns the payment intent Id stored in the current session.
    /// </summary>
    string Retrieve();

    /// <summary>
    /// Saves a payment intent Id to the session.
    /// </summary>
    void Store(string paymentIntentId);

    /// <summary>
    /// Removes the payment intent Id stored in the current session.
    /// </summary>
    void Remove();
}
