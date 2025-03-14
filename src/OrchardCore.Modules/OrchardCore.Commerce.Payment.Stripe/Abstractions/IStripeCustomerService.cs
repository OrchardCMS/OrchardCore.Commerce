using Stripe;
using System.Threading.Tasks;
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Stripe customer related services.
/// </summary>
public interface IStripeCustomerService
{
    /// <summary>
    /// Search for customers in Stripe with the given <paramref name="options"/>.
    /// </summary>
    Task<StripeSearchResult<Customer>> SearchCustomersAsync(CustomerSearchOptions options);

    /// <summary>
    /// Get the first customer with the given email in Stripe.
    /// </summary>
    Task<Customer> GetFirstCustomerByEmailAsync(string customerEmail);

    /// <summary>
    /// Returns <see cref="Customer"/> with the given Id in Stripe.
    /// </summary>
    Task<Customer> GetCustomerByIdAsync(string customerId);

    /// <summary>
    /// Returns <see cref="Customer"/> with the given email in Stripe. If not found, create a new customer.
    /// </summary>
    /// <param name="email">If not provided the current user's email will be used.</param>
    Task<Customer> GetAndUpdateOrCreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);

    /// <summary>
    /// Create a new customer in Stripe with the given <paramref name="customerCreateOptions"/>.
    /// </summary>
    /// <returns>The created Stripe <see cref="Customer"/>.</returns>
    Task<Customer> CreateCustomerAsync(CustomerCreateOptions customerCreateOptions);

    /// <summary>
    /// Create the customer in Stripe with the given details which will be used to create the
    /// <see cref="CustomerCreateOptions"/>.
    /// </summary>
    /// <returns>The created Stripe <see cref="Customer"/>.</returns>
    Task<Customer> CreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);

    /// <summary>
    /// Update the customer in Stripe with the given details.
    /// </summary>
    /// <returns>The updated Stripe <see cref="Customer"/>.</returns>
    Task<Customer> UpdateCustomerAsync(
        string customerId,
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);

    /// <summary>
    /// Populate the returned <see cref="CustomerCreateOptions"/> with the given details.
    /// </summary>
    CustomerCreateOptions PopulateCustomerCreateOptions(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone);
}
