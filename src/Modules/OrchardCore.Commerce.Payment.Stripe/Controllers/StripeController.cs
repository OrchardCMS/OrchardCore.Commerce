using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.Utilities;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

public class StripeController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IPaymentService _paymentService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IHtmlLocalizer<StripeController> H;
    public StripeController(
        IContentManager contentManager,
        INotifier notifier,
        IPaymentIntentPersistence paymentIntentPersistence,
        IPaymentService paymentService,
        IStripePaymentService stripePaymentService,
        IHtmlLocalizer<StripeController> htmlLocalizer)
    {
        _contentManager = contentManager;
        _notifier = notifier;
        _paymentIntentPersistence = paymentIntentPersistence;
        _paymentService = paymentService;
        _stripePaymentService = stripePaymentService;
        H = htmlLocalizer;
    }

    [AllowAnonymous]
    [HttpGet("checkout/middleware")]
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
        var finished = succeeded && status == OrderStatuses.Ordered.HtmlClassify();

        if (succeeded && status == OrderStatuses.Pending.HtmlClassify())
        {
            await _stripePaymentService.UpdateOrderToOrderedAsync(fetchedPaymentIntent);
            finished = true;
        }

        if (finished)
        {
            await _paymentService.FinalModificationOfOrderAsync(
                order,
                shoppingCartId: null,
                StripePaymentProvider.ProviderName);
            return RedirectToAction(
                nameof(PaymentController.Success),
                typeof(PaymentController).ControllerName(),
                new { area = OrchardCore.Commerce.Payment.Constants.FeatureIds.Area, orderId });
        }

        var errorMessage = H["The payment failed, please try again."];
        if (status == OrderStatuses.PaymentFailed.HtmlClassify())
        {
            await _notifier.ErrorAsync(errorMessage);
            return Redirect("~/checkout");
        }

        order.Alter<StripePaymentPart>(part => part.RetryCounter++);
        await _contentManager.UpdateAsync(order);

        if (order.As<StripePaymentPart>().RetryCounter <= 10)
        {
            return View();
        }

        // Delete payment intent from session, to create a new one.
        _paymentIntentPersistence.Store(string.Empty);
        await _notifier.ErrorAsync(errorMessage);
        return Redirect("~/checkout");
    }
}
