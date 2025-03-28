using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System.Net.Mime;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

public class StripeController : PaymentBaseController
{
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IStripePaymentService _stripePaymentService;

    public StripeController(
        IOrchardServices<StripeController> services,
        INotifier notifier,
        IPaymentIntentPersistence paymentIntentPersistence,
        IStripePaymentService stripePaymentService)
        : base(notifier, services.Logger.Value)
    {
        _paymentIntentPersistence = paymentIntentPersistence;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<IActionResult> UpdatePaymentIntent(string paymentIntent)
    {
        await _paymentIntentPersistence.StoreAsync(paymentIntent);
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("stripe/middleware")]
    public async Task<IActionResult> PaymentConfirmation(
        [FromQuery(Name = "payment_intent")] string paymentIntent = null,
        [FromQuery] string shoppingCartId = null)
    {
        var result = await _stripePaymentService.PaymentConfirmationAsync(paymentIntent, shoppingCartId);
        return await ProduceActionResultAsync(result);
    }

    [HttpPost("stripe/params")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentParameters()
    {
        var middlewareUrl = Url.ToAbsoluteUrl("~/stripe/middleware");
        var model = await _stripePaymentService.GetStripeConfirmParametersAsync(middlewareUrl);

        // Newtonsoft is used, because the external Stripe library that defined PaymentIntentConfirmOptions does not
        // support System.Text.Json.
        var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        return Content(json, MediaTypeNames.Application.Json);
    }
}
