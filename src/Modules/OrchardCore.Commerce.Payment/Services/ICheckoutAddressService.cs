using OrchardCore.Commerce.Payment.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Services;

/// <summary>
/// Defines a service for handling checkout address-related operations.
/// </summary>
public interface ICheckoutAddressService
{
    /// <summary>
    /// Determines whether the specified address in the checkout process should be ignored.
    /// </summary>
    /// <param name="checkoutViewModel">The view model containing checkout details, including the address to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the address
    /// should be ignored; otherwise, <see langword="false"/>.</returns>
    Task<bool> ShouldIgnoreAddressAsync(CheckoutViewModel checkoutViewModel);
}
