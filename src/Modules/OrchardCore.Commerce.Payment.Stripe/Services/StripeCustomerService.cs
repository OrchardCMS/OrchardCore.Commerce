using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System.Linq;
using System.Threading.Tasks;
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeCustomerService : IStripeCustomerService
{
    private readonly IHttpContextAccessor _hca;
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly CustomerService _customerService;
    private readonly ICachingUserManager _cachingUserManager;

    public StripeCustomerService(
        CustomerService customerService,
        IHttpContextAccessor httpContextAccessor,
        IRequestOptionsService requestOptionsService,
        ICachingUserManager cachingUserManager)
    {
        _customerService = customerService;
        _hca = httpContextAccessor;
        _requestOptionsService = requestOptionsService;
        _cachingUserManager = cachingUserManager;
    }

    public async Task<StripeSearchResult<Customer>> SearchCustomersAsync(CustomerSearchOptions options)
    {
        var list = await _customerService.SearchAsync(
            options,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);
        return list;
    }

    public async Task<Customer> GetCustomerByIdAsync(string customerId) =>
        await _customerService.GetAsync(
            customerId,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

    public async Task<Customer> GetFirstCustomerByEmailAsync(string customerEmail)
    {
        var list = await _customerService.ListAsync(
            new CustomerListOptions
            {
                Email = customerEmail,
                Limit = 1,
            },
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);
        return list.Data.FirstOrDefault();
    }

    public async Task<Customer> GetAndUpdateOrCreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone)
    {
        if (string.IsNullOrEmpty(email))
        {
            email = (await _cachingUserManager.GetUserByClaimsPrincipalAsync(_hca.HttpContext.User))?.Email;
        }

        var customer = await GetFirstCustomerByEmailAsync(email);

        if (customer?.Id != null)
        {
            customer = await UpdateCustomerAsync(customer.Id, billingAddress, shippingAddress, email, phone);
            return customer;
        }

        return await CreateCustomerAsync(billingAddress, shippingAddress, email, phone);
    }

    public async Task<Customer> CreateCustomerAsync(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone)
    {
        var customerCreateOptions = PopulateCustomerCreateOptions(billingAddress, shippingAddress, email, phone);

        var customer = await _customerService.CreateAsync(
            customerCreateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

        return customer;
    }

    public async Task<Customer> CreateCustomerAsync(CustomerCreateOptions customerCreateOptions)
    {
        var customer = await _customerService.CreateAsync(
            customerCreateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(
        string customerId,
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone)
    {
        var customerUpdateOptions = PopulateCustomerUpdateOptions(billingAddress, shippingAddress, email, phone);

        var customer = await _customerService.UpdateAsync(
            customerId,
            customerUpdateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

        return customer;
    }

    private static CustomerUpdateOptions PopulateCustomerUpdateOptions(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone) =>
        new()
        {
            Name = billingAddress.Name,
            Email = email,
            Phone = phone,
            Address = CreateAddressOptions(billingAddress),
            Shipping = CreateShippingOptions(shippingAddress),
        };

    public CustomerCreateOptions PopulateCustomerCreateOptions(
        Address billingAddress,
        Address shippingAddress,
        string email,
        string phone) =>
        new()
        {
            Name = billingAddress.Name,
            Email = email,
            Phone = phone,
            Address = CreateAddressOptions(billingAddress),
            Shipping = CreateShippingOptions(shippingAddress),
        };

    private static AddressOptions CreateAddressOptions(Address address) => new()
    {
        City = address.City,
        Country = address.Region,
        Line1 = address.StreetAddress1,
        Line2 = address.StreetAddress2,
        PostalCode = address.PostalCode,
        State = address.Province,
    };

    private static ShippingOptions CreateShippingOptions(Address shippingAddress) =>
        shippingAddress?.Name != null
            ? new ShippingOptions
            {
                Name = shippingAddress.Name,
                Address = CreateAddressOptions(shippingAddress),
            }
            : null;
}
