using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using Stripe;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

[Route("stripe-webhook")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public class WebhookController : Controller
{
    // This is a default test Stripe CLI webhook secret for testing your endpoint locally. It can be shown, as this is
    // the same always for testing in Stripe.
    private const string LocalEndPointSecret =
        "whsec_453d1046fc31377b7a93e82b839c9e6e065d7117b6e02422e55eac99da087463";

    private readonly IStripePaymentService _stripePaymentService;
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IStripePaymentService stripePaymentService,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<WebhookController> logger)
    {
        _stripePaymentService = stripePaymentService;
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        using var streamReader = new StreamReader(HttpContext.Request.Body);
        var json = await streamReader.ReadToEndAsync();
        try
        {
            // If the webhook signing key is empty, default to the test key.
            var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();
            var webhookSigningKey = stripeApiSettings.DecryptWebhookSigningSecret(_dataProtectionProvider, _logger)
                .OrIfEmpty(LocalEndPointSecret);

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSigningKey);

            if (stripeEvent.Type == Stripe.Events.ChargeSucceeded)
            {
                var charge = stripeEvent.Data.Object as Charge;
                if (charge?.PaymentIntentId is not { } paymentIntentId)
                {
                    return BadRequest();
                }

                var paymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId);
                await _stripePaymentService.UpdateOrderToOrderedAsync(paymentIntent);
            }
            else if (stripeEvent.Type == Stripe.Events.PaymentIntentPaymentFailed)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                await _stripePaymentService.UpdateOrderToPaymentFailedAsync(paymentIntent);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e);
        }
    }
}
