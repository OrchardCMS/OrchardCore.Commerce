using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using Stripe;
using System.Threading.Tasks;
using Address = OrchardCore.Commerce.AddressDataType.Address;

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
            await _notifier.ErrorAsync(
                H["Couldn't find the payment intent \"{0}\" or the order associated with it.", paymentIntent ?? string.Empty]);
            return NotFound();
        }

        var part = order.As<OrderPart>() ?? new OrderPart();
        var succeeded = fetchedPaymentIntent.Status == PaymentIntentStatuses.Succeeded;

        // Looks like there is nothing to do here.
        if (succeeded && part.IsOrdered)
        {
            return this.RedirectToContentDisplay(order);
        }

        if (succeeded && part.IsPending)
        {
            return await _stripePaymentService.UpdateAndRedirectToFinishedOrderAsync(
                this,
                order,
                fetchedPaymentIntent,
                shoppingCartId);
        }

        if (part.IsFailed)
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

    [HttpPost("checkout/params/Stripe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetConfirmPaymentParameters()
    {
        var order = await _contentManager.NewAsync(Commerce.Abstractions.Constants.ContentTypes.Order);
        await _paymentService.UpdateOrderWithDriversAsync(order);

        var part = order.As<OrderPart>();
        var billing = part.BillingAddress.Address ?? new Address();
        var shipping = part.ShippingAddress.Address ?? new Address();

        var model = new PaymentIntentConfirmOptions
        {
            ReturnUrl = Url.ToAbsoluteUrl("~/checkout/middleware/Stripe"),
            PaymentMethodData = new PaymentIntentPaymentMethodDataOptions
            {
                BillingDetails = new PaymentIntentPaymentMethodDataBillingDetailsOptions
                {
                    Email = part.Email?.Text,
                    Name = billing.Name,
                    Phone = part.Phone?.Text,
                    Address = CreateAddressOptions(billing),
                },
            },
            Shipping = new ChargeShippingOptions
            {
                Name = shipping.Name,
                Phone = part.Phone?.Text,
                Address = CreateAddressOptions(shipping),
            },
        };

        return Json(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
    }

    private async Task<IActionResult> PaymentFailedAsync()
    {
        await _notifier.ErrorAsync(H["The has payment failed, please try again."]);
        return Redirect("~/checkout");
    }

    private static AddressOptions CreateAddressOptions(Address address) =>
        new()
        {
            City = address.City ?? string.Empty,
            Country = address.Region ?? string.Empty,
            Line1 = address.StreetAddress1 ?? string.Empty,
            Line2 = address.StreetAddress2 ?? string.Empty,
            PostalCode = address.PostalCode ?? string.Empty,
            State = address.Province ?? string.Empty,
        };
}
