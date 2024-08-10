using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

public class StripeController : PaymentBaseController
{
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IHtmlLocalizer<StripeController> H;

    public StripeController(
        INotifier notifier,
        IPaymentIntentPersistence paymentIntentPersistence,
        IStripePaymentService stripePaymentService,
        IHtmlLocalizer<StripeController> htmlLocalizer)
        : base(notifier)
    {
        _paymentIntentPersistence = paymentIntentPersistence;
        _stripePaymentService = stripePaymentService;
        H = htmlLocalizer;
    }

    public IActionResult UpdatePaymentIntent(string paymentIntent)
    {
        _paymentIntentPersistence.Store(paymentIntent);
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("checkout/middleware/Stripe")]
    public async Task<IActionResult> PaymentConfirmationMiddleware(
        [FromQuery(Name = "payment_intent")] string paymentIntent = null,
        [FromQuery] string shoppingCartId = null)
    {
        var result = await _stripePaymentService.PaymentConfirmationAsync(paymentIntent, shoppingCartId, H);
        return await ProduceResultAsync(result);
    }

    [HttpPost("checkout/params/Stripe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetConfirmPaymentParameters()
    {
        var middlewareUrl = Url.ToAbsoluteUrl("~/checkout/middleware/Stripe");
        var model = await _stripePaymentService.GetStripeConfirmParametersAsync(middlewareUrl);
        return Json(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
    }
}
