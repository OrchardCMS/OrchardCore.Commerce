using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Commerce.Payment.Stripe.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Helpers;
using OrchardCore.Settings;
using Stripe;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Constants.CurrencyCollectionConstants;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripePaymentIntentService : IStripePaymentIntentService
{
    private readonly PaymentIntentService _paymentIntentService;
    private readonly IHttpContextAccessor _hca;
    private readonly IRequestOptionsService _requestOptionsService;
    private readonly ISiteService _siteService;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IStringLocalizer<StripePaymentIntentService> T;

    public StripePaymentIntentService(
        PaymentIntentService paymentIntentService,
        IHttpContextAccessor httpContextAccessor,
        IRequestOptionsService requestOptionsService,
        ISiteService siteService,
        IPaymentIntentPersistence paymentIntentPersistence,
        IStringLocalizer<StripePaymentIntentService> localizer)
    {
        _paymentIntentService = paymentIntentService;
        _hca = httpContextAccessor;
        _requestOptionsService = requestOptionsService;
        _siteService = siteService;
        _paymentIntentPersistence = paymentIntentPersistence;
        T = localizer;
    }

    public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentGetOptions = new PaymentIntentGetOptions();
        paymentIntentGetOptions.AddExpansions();
        return await _paymentIntentService.GetAsync(
            paymentIntentId,
            paymentIntentGetOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            _hca.HttpContext.RequestAborted);
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(Amount total)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = AmountHelpers.GetPaymentAmount(total),
            Currency = total.Currency.CurrencyIsoCode,
            Description = T["User checkout on {0}", siteSettings.SiteName].Value,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true, },
        };

        var paymentIntent = await CreatePaymentIntentAsync(paymentIntentOptions);

        _paymentIntentPersistence.Store(paymentIntent.Id);

        return paymentIntent;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options) =>
        await _paymentIntentService.CreateAsync(
            options,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            _hca.HttpContext.RequestAborted);

    public async Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        Amount defaultTotal)
    {
        var paymentIntent = await GetPaymentIntentAsync(paymentIntentId);

        if (paymentIntent?.Status is PaymentIntentStatuses.Succeeded or PaymentIntentStatuses.Processing)
        {
            return paymentIntent;
        }

        var updateOptions = new PaymentIntentUpdateOptions
        {
            Amount = AmountHelpers.GetPaymentAmount(defaultTotal),
            Currency = defaultTotal.Currency.CurrencyIsoCode,
        };

        updateOptions.AddExpansions();
        return await _paymentIntentService.UpdateAsync(
            paymentIntentId,
            updateOptions,
            await _requestOptionsService.SetIdempotencyKeyAsync(),
            _hca.HttpContext.RequestAborted);
    }
}
