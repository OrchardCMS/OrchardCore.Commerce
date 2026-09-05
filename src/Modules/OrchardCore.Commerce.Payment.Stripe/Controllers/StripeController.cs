using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

public class StripeController : PaymentBaseController
{
    public StripeController(
        IOrchardServices<StripeController> services,
        INotifier notifier)
        : base(notifier, services.Logger.Value)
    {
    }

    [AllowAnonymous]
    [HttpGet("stripe/middleware")]
    public async Task<IActionResult> PaymentConfirmation(
        [FromQuery(Name = "payment_intent")] string paymentIntent = null,
        [FromQuery] string shoppingCartId = null,
        [FromQuery] string environment = null)
    {
        var stripePaymentService = HttpContext.GetRequiredKeyedPaymentService<IStripePaymentService>(environment);
        var result = await stripePaymentService.PaymentConfirmationAsync(paymentIntent, shoppingCartId);
        return await ProduceActionResultAsync(result);
    }

    [HttpPost("stripe/params")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentParameters([FromQuery] string environment = null)
    {
        var paymentEnvironment = PaymentEnvironmentExtensions.Parse(environment);
        var stripePaymentService = HttpContext.GetRequiredKeyedPaymentService<IStripePaymentService>(environment);
        var middlewareUrl = Url.ToAbsoluteUrl("~/stripe/middleware");
        if (paymentEnvironment.ToQueryValue() is { } queryValue)
        {
            middlewareUrl += $"?{PaymentEnvironmentExtensions.QueryParameterName}={queryValue}";
        }

        var model = await stripePaymentService.GetStripeConfirmParametersAsync(middlewareUrl);

        // Newtonsoft is used, because the external Stripe library that defined PaymentIntentConfirmOptions does not
        // support System.Text.Json.
        var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        return Content(json, MediaTypeNames.Application.Json);
    }
}
