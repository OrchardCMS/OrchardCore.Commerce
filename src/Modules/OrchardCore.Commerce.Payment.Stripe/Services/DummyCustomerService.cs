using Stripe;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class DummyCustomerService : CustomerService
{
    public const string TestCustomerId = "cus_TESTID00000000";

    public override Customer Create(CustomerCreateOptions options, RequestOptions requestOptions = null) => new();

    public override Task<Customer> CreateAsync(
        CustomerCreateOptions options,
        RequestOptions requestOptions = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new Customer { Id = TestCustomerId });

    public override Task<StripeSearchResult<Customer>> SearchAsync(
        CustomerSearchOptions options = null,
        RequestOptions requestOptions = null,
        CancellationToken cancellationToken = default) => Task.FromResult<StripeSearchResult<Customer>>(null);
}
