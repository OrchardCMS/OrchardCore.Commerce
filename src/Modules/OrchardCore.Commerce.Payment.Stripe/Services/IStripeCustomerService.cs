using Stripe;
using System.Threading.Tasks;
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeCustomerService
{
    Task<Customer> GetFirstCustomerByEmailAsync(string customerEmail);
    Task<Customer> GetCustomerByIdAsync(string customerId);
    Task<Customer> GetOrCreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);
}
