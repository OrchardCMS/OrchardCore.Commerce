using Lombiq.HelpfulLibraries.OrchardCore.Users;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeCustomerService : IStripeCustomerService
{
    private readonly IHttpContextAccessor _hca;
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly CustomerService _customerService = new();
    private readonly ICachingUserManager _cachingUserManager;

    public StripeCustomerService(
        IHttpContextAccessor httpContextAccessor,
        IRequestOptionsService requestOptionsService,
        ICachingUserManager cachingUserManager)
    {
        _hca = httpContextAccessor;
        _requestOptionsService = requestOptionsService;
        _cachingUserManager = cachingUserManager;
    }

    public async Task<Customer> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            return await _customerService.GetAsync(
                customerId,
                requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync(),
                cancellationToken: _hca.HttpContext.RequestAborted);
        }
        catch (StripeException stripeException)
        {
            return null;
        }
    }

    public async Task<Customer> GetFirstCustomerByEmailAsync(string customerEmail)
    {
        try
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
        catch (StripeException stripeException)
        {

            return null;
        }
    }

    public async Task<Customer> GetOrCreateCustomerAsync(
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
            return customer;
        }

        return await CreateCustomerAsync(billingAddress, shippingAddress, email, phone);
    }

    private async Task<Customer> CreateCustomerAsync(Address billingAddress, Address shippingAddress, string email, string phone)
    {
        var customerCreateOptions = new CustomerCreateOptions
        {
            Name = billingAddress.Name,
            Email = email,
            Phone = phone,
            Address = new AddressOptions
            {
                City = billingAddress.City,
                Country = billingAddress.Region,
                Line1 = billingAddress.StreetAddress1,
                Line2 = billingAddress.StreetAddress2,
                PostalCode = billingAddress.PostalCode,
                State = billingAddress.Province,
            },
            Shipping = shippingAddress?.Name != null
                ? new ShippingOptions
                    {
                        Name = shippingAddress.Name,
                        Address = new AddressOptions
                        {
                            City = shippingAddress.City,
                            Country = shippingAddress.Region,
                            Line1 = shippingAddress.StreetAddress1,
                            Line2 = shippingAddress.StreetAddress2,
                            PostalCode = shippingAddress.PostalCode,
                            State = shippingAddress.Province,
                        },
                    }
                : null,
        };

        var customer = await _customerService.CreateAsync(
            customerCreateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: _hca.HttpContext.RequestAborted);

        return customer;
    }
}
