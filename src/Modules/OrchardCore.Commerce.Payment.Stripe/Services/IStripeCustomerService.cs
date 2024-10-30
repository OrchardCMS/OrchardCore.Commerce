using Stripe;
using System.Threading.Tasks;
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeCustomerService
{
    Task<Customer> CreateCustomerAsync(CustomerCreateOptions customerCreateOptions);
    Task<Customer> GetFirstCustomerByEmailAsync(string customerEmail);
    Task<Customer> GetCustomerByIdAsync(string customerId);
    Task<Customer> GetAndUpdateOrCreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);

    Task<Customer> CreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);

    Task<Customer> UpdateCustomerAsync(
        string customerId,
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);
}
