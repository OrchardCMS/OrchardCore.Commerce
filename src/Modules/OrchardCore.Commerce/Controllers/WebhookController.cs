using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

[Route("stripe-webhook")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public class WebhookController : Controller
{
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
            var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();
            var webhookSigningKey = stripeApiSettings.DecryptWebhookSigningSecret(_dataProtectionProvider, _logger);

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSigningKey,
                throwOnApiVersionMismatch: false);

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
