using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Settings;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

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
        var json = await streamReader.ReadToEndAsync(HttpContext.RequestAborted);
        try
        {
            var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();
            var webhookSigningKey = stripeApiSettings.DecryptWebhookSigningSecret(_dataProtectionProvider, _logger);

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSigningKey,
                // Let the logic handle version mismatch.
                throwOnApiVersionMismatch: false);

            if (stripeEvent.Type == Events.ChargeSucceeded)
            {
                var charge = stripeEvent.Data.Object as Charge;
                if (charge?.PaymentIntentId is not { } paymentIntentId)
                {
                    return BadRequest();
                }

                var paymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId);
                await _stripePaymentService.UpdateOrderToOrderedAsync(paymentIntent, shoppingCartId: null);
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                await _stripePaymentService.UpdateOrderToPaymentFailedAsync(paymentIntent.Id);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e);
        }
    }
}
