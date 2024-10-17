using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public IActionResult UpdatePaymentIntent(string paymentIntent)
    {
        _paymentIntentPersistence.Store(paymentIntent);
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("checkout/middleware/Stripe")]
    [Obsolete("This endpoint is obsolete and will be removed in a future version. Use checkout/stripe/middleware instead.")]
    public Task<IActionResult> PaymentConfirmationMiddleware(
        [FromQuery(Name = "payment_intent")] string paymentIntent = null,
        [FromQuery] string shoppingCartId = null) => PaymentConfirmation(paymentIntent, shoppingCartId);

    [HttpPost("checkout/params/Stripe")]
    [ValidateAntiForgeryToken]
    [Obsolete("This endpoint is obsolete and will be removed in a future version. Use checkout/stripe/params instead.")]
    public Task<IActionResult> GetConfirmPaymentParameters() => ConfirmPaymentParameters();

    [AllowAnonymous]
    [HttpGet("checkout/stripe/middleware")]
    public async Task<IActionResult> PaymentConfirmation(
        [FromQuery(Name = "payment_intent")] string paymentIntent = null,
        [FromQuery] string shoppingCartId = null)
    {
        var result = await _stripePaymentService.PaymentConfirmationAsync(paymentIntent, shoppingCartId);
        return await ProduceActionResultAsync(result);
    }

    [HttpPost("checkout/stripe/params")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentParameters()
    {
        var middlewareUrl = Url.ToAbsoluteUrl("~/checkout/stripe/params");
        var model = await _stripePaymentService.GetStripeConfirmParametersAsync(middlewareUrl);
        return Json(model, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
    }
}
