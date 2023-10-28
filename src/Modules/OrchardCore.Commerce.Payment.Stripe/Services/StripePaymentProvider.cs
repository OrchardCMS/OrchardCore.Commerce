using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Threading.Tasks;
using ISession=YesSql.ISession;

namespace OrchardCore.Commerce.Services;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly IHttpContextAccessor _hca;
    private readonly ISession _session;
    private readonly ISiteService _siteService;
    private readonly IStripePaymentService _stripePaymentService;

    public string Name => "Stripe";

    public StripePaymentProvider(
        IHttpContextAccessor hca,
        ISession session,
        ISiteService siteService,
        IStripePaymentService stripePaymentService)
    {
        _hca = hca;
        _session = session;
        _siteService = siteService;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model)
    {
        var paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(model.SingleCurrencyTotal);

        if (_hca.HttpContext?.GetRouteValue("action")?.ToString() == nameof(PaymentController.PaymentRequest))
        {
            _session.Save(new OrderPayment
            {
                OrderId = model.OrderPart.ContentItem.ContentItemId,
                PaymentIntentId = paymentIntent.Id,
            });
        }

        return new
        {
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            PaymentIntentClientSecret = paymentIntent.ClientSecret,
        };
    }
}
