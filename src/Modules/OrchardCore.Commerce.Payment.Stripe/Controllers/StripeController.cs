using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Controllers;

public class StripeController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly IOrchardHelper _orchardHelper;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IHtmlLocalizer<StripeController> H;

    public StripeController(
        IContentManager contentManager,
        INotifier notifier,
        IOrchardHelper orchardHelper,
        IPaymentIntentPersistence paymentIntentPersistence,
        IStripePaymentService stripePaymentService,
        IHtmlLocalizer<StripeController> htmlLocalizer)
    {
        _contentManager = contentManager;
        _notifier = notifier;
        _orchardHelper = orchardHelper;
        _paymentIntentPersistence = paymentIntentPersistence;
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

        // Looks like there is nothing to do here.
        if (succeeded && status == OrderStatuses.Ordered.HtmlClassify())
        {
            return Redirect(_orchardHelper.GetItemDisplayUrl(orderId));
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
        _paymentIntentPersistence.Store(string.Empty);
        return await PaymentFailedAsync();
    }

    private async Task<IActionResult> PaymentFailedAsync()
    {
        await _notifier.ErrorAsync(H["The payment failed, please try again."]);
        return Redirect("~/checkout");
    }
}
