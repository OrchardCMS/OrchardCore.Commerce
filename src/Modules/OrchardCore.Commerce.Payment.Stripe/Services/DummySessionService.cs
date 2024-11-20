using Stripe;
using Stripe.Checkout;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class DummySessionService : SessionService
{
    public const string TestSessionId = "cs_test_testsessionid000000000000000000000000000000000000000000000";
    public const string TestSessionUrl = "https://localhost";

    public override Session Create(SessionCreateOptions options, RequestOptions requestOptions = null) => new();

    public override Task<Session> CreateAsync(
        SessionCreateOptions options,
        RequestOptions requestOptions = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new Session { Id = TestSessionId, Url = TestSessionUrl });
}
