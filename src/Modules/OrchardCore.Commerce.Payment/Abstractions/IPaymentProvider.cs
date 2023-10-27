using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Payment.Abstractions;

/// <summary>
/// A payment provider that can be used during checkout to add a charge to the order.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Gets the name used to identify additional payment provider data in the <see cref="ICheckoutViewModel"/> and as a
    /// suffix to the <c>Checkout-{Name}</c> shape used to display the payment UI during checkout or in the order view.
    /// </summary>
    string Name { get; }
}
