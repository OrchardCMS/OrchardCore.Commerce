using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class RequestOptionsService : IRequestOptionsService
{
    private readonly ISiteService _siteService;

    private readonly Func<ISite, string> _apiKeyAccessor;
    private RequestOptions _requestOptions;

    public RequestOptionsService(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<RequestOptionsService> logger)
    {
        _siteService = siteService;

        _apiKeyAccessor = siteSettings => siteSettings
            .As<StripeApiSettings>()
            .SecretKey
            .DecryptStripeApiKey(dataProtectionProvider, logger);
    }

    public Task<RequestOptions> GetOrCreateRequestOptionsAsync() =>
        _requestOptions == null ? CreateRequestOptionsAsync() : Task.FromResult(_requestOptions);

    public async Task<RequestOptions> SetIdempotencyKeyAsync()
    {
        var requestOptions = await GetOrCreateRequestOptionsAsync();
        requestOptions.IdempotencyKey = Guid.NewGuid().ToString();

        return requestOptions;
    }

    private async Task<RequestOptions> CreateRequestOptionsAsync()
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var apiKey = _apiKeyAccessor(siteSettings);

        _requestOptions = new RequestOptions { ApiKey = apiKey };
        return _requestOptions;
    }
}
