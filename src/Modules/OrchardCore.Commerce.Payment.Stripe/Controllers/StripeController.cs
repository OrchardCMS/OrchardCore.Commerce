using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

public class StripeController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IHtmlLocalizer<StripeController> H;

    public StripeController(
        IContentManager contentManager,
        INotifier notifier,
        IPaymentIntentPersistence paymentIntentPersistence,
        IStripePaymentService stripePaymentService,
        IHtmlLocalizer<StripeController> htmlLocalizer)
    {
        _contentManager = contentManager;
        _notifier = notifier;
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
    public async Task<IActionResult> PaymentConfirmationMiddleware([FromQuery(Name = "payment_intent")] string paymentIntent = null)
    {
        // If it is null it means the session was not loaded yet and a redirect is needed.
        if (string.IsNullOrEmpty(_paymentIntentPersistence.Retrieve()))
        {
            return View();
        }

        // If we can't find a valid payment intent based on ID or if we can't find the associated order, then something
        // went wrong and continuing from here would only cause a crash anyway.
        if (await _stripePaymentService.GetPaymentIntentAsync(paymentIntent) is not { PaymentMethod: not null } fetchedPaymentIntent ||
            (await _stripePaymentService.GetOrderPaymentByPaymentIntentIdAsync(paymentIntent))?.OrderId is not { } orderId ||
            await _contentManager.GetAsync(orderId) is not { } order)
        {
            return NotFound();
        }

        var status = order.As<OrderPart>()?.Status?.Text;
        var succeeded = fetchedPaymentIntent.Status == PaymentIntentStatuses.Succeeded;

        // Looks like there is nothing to do here.
        if (succeeded && status == OrderStatuses.Ordered.HtmlClassify())
        {
            return this.RedirectToContentDisplay(order);
        }

        if (succeeded && status == OrderStatuses.Pending.HtmlClassify())
        {
            return await _stripePaymentService.UpdateAndRedirectToFinishedOrderAsync(
                this,
                order,
                fetchedPaymentIntent);
        }

        if (status == OrderStatuses.PaymentFailed.HtmlClassify())
        {
            return await PaymentFailedAsync();
        }

        order.Alter<StripePaymentPart>(part => part.RetryCounter++);
        await _contentManager.UpdateAsync(order);

        if (order.As<StripePaymentPart>().RetryCounter <= 10)
        {
            return View();
        }

        // Delete payment intent from session, to create a new one.
        _paymentIntentPersistence.Remove();
        return await PaymentFailedAsync();
    }

    private async Task<IActionResult> PaymentFailedAsync()
    {
        await _notifier.ErrorAsync(H["The has payment failed, please try again."]);
        return Redirect("~/checkout");
    }
}
