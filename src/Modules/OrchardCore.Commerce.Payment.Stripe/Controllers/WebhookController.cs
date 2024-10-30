using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Settings;
using Stripe;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

[Route("stripe-webhook")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public class WebhookController : Controller
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<WebhookController> _logger;
    private readonly IEnumerable<IStripeWebhookEventHandler> _stripeWebhookEventHandlers;

    public WebhookController(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<WebhookController> logger,
        IEnumerable<IStripeWebhookEventHandler> stripeWebhookEventHandlers)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
        _stripeWebhookEventHandlers = stripeWebhookEventHandlers;
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
            if (string.IsNullOrEmpty(stripeEvent.Id))
            {
                throw new StripeException("Invalid event or event Id.");
            }

            await _stripeWebhookEventHandlers.AwaitEachAsync(handler => handler.ReceivedStripeEventAsync(stripeEvent));

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e);
        }
    }
}
