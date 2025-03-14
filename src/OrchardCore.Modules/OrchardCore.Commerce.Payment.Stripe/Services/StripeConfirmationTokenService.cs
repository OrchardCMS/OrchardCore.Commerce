using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeConfirmationTokenService : IStripeConfirmationTokenService
{
    private readonly ConfirmationTokenService _confirmationTokenService;
    private readonly IHttpContextAccessor _hca;
    private readonly IRequestOptionsService _requestOptionsService;

    public StripeConfirmationTokenService(
        ConfirmationTokenService confirmationTokenService,
        IHttpContextAccessor httpContextAccessor,
        IRequestOptionsService requestOptionsService)
    {
        _confirmationTokenService = confirmationTokenService;
        _hca = httpContextAccessor;
        _requestOptionsService = requestOptionsService;
    }

    public async Task<ConfirmationToken> GetConfirmationTokenAsync(string confirmationTokenId) =>
        await _confirmationTokenService.GetAsync(
            confirmationTokenId,
            cancellationToken: _hca.HttpContext.RequestAborted,
            requestOptions: await _requestOptionsService.SetIdempotencyKeyAsync());
}
