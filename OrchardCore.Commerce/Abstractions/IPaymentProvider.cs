using System.Collections.Generic;
using Money;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// When implemented, this service creates and populates <see cref="IPayment"/>.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Creates a new payment, if the payment is of the expected <paramref name="kind"/>.
    /// </summary>
    /// <param name="kind">This value indicates towards the provider if it's applicable.</param>
    /// <param name="transactionId">A unique ID.</param>
    /// <param name="amount">The monetary amount to charge through the payment method.</param>
    /// <param name="data">A collection of additional data specific to the payment provider.</param>
    /// <returns>A new instance of <see cref="IPayment"/> or <see langword="null"/> if not applicable.</returns>
    public IPayment CreateCharge(string kind, string transactionId, Amount amount, IDictionary<string, string> data);

    /// <summary>
    /// Converts the information from <paramref name="charge"/> into <paramref name="data"/> so it can be used to
    /// create further charges.
    /// </summary>
    public void AddData(IPayment charge, IDictionary<string, string> data);
}
