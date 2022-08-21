using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Controllers;

public class PaymentController : Controller
{
    private readonly ICardPaymentService _cardPaymentService;
    private readonly IStringLocalizer T;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;

    public PaymentController(
        ICardPaymentService cardPaymentService,
        IStringLocalizer<PaymentController> stringLocalizer,
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca)
    {
        _cardPaymentService = cardPaymentService;
        _authorizationService = authorizationService;
        _hca = hca;
        T = stringLocalizer;
    }

    [Route("checkout")]
    public async Task<IActionResult> Index()
    {
        var user = _hca.HttpContext?.User;

        if (user != null && !await _authorizationService.AuthorizeAsync(user, Permissions.Checkout))
        {
            return user.Identity.IsAuthenticated ? Forbid() : LocalRedirect("~/Login?ReturnUrl=~/checkout");
        }

        return View();
    }

    [Route("success")]
    public IActionResult Success() =>
        View();

    [Route("pay")]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Pay([FromBody] ConfirmPaymentRequest request)
    {
        PaymentIntent paymentIntent;

        try
        {
            paymentIntent = await _cardPaymentService.CreatePaymentAsync(request);
        }
        catch (StripeException exception)
        {
            return Json(new { error = exception.StripeError.Message });
        }

        return await GeneratePaymentResponseAsync(paymentIntent);
    }

    private async Task<IActionResult> GeneratePaymentResponseAsync(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Status == "requires_action" &&
            paymentIntent.NextAction.Type == "use_stripe_sdk")
        {
            // Tell the client to handle the action.
            return Json(new
            {
                requires_action = true,
                payment_intent_client_secret = paymentIntent.ClientSecret,
            });
        }

        if (paymentIntent.Status == "succeeded")
        {
            // The payment didn’t need any additional actions and completed!
            // Create the order content item.
            await _cardPaymentService.CreateOrderFromShoppingCartAsync(paymentIntent);

            return Json(new { success = true });
        }

        // Invalid status.
        return StatusCode(StatusCodes.Status500InternalServerError, new { error = T["Invalid PaymentIntent status"].Value });
    }
}
