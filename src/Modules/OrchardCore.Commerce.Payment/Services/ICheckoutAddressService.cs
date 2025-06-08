using OrchardCore.Commerce.Payment.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Services;
public interface ICheckoutAddressService
{
    Task<bool> ShouldIgnoreAddressAsync(CheckoutViewModel checkoutViewModel);
}