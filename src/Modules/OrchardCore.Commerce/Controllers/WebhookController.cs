using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using Stripe;
using System.IO;
using System.Threading.Tasks;

[Route("webhook")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public class WebhookController : Controller
{
    private readonly IStripePaymentService _stripePaymentService;

    // This is your Stripe CLI webhook secret for testing your endpoint locally.
    const string endpointSecret = "whsec_453d1046fc31377b7a93e82b839c9e6e065d7117b6e02422e55eac99da087463";

    public WebhookController(IStripePaymentService stripePaymentService)
    {
        _stripePaymentService = stripePaymentService;
    }

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                endpointSecret);

            if (stripeEvent.Type != Stripe.Events.ChargeSucceeded)
            {
                return Ok();
            }

            var charge = stripeEvent.Data.Object as Charge;
            if (charge?.PaymentIntentId is not { } paymentIntentId)
            {
                return BadRequest();
            }

            var paymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId);
            await _stripePaymentService.UpdateOrderToOrderedAsync(paymentIntent, charge);

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e);
        }
    }
}
