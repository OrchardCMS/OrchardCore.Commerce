using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Threading.Tasks;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripePaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Stripe";

    private readonly IHttpContextAccessor _hca;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly ISession _session;
    private readonly ISiteService _siteService;
    private readonly IStripePaymentService _stripePaymentService;

    public string Name => ProviderName;

    public StripePaymentProvider(
        IHttpContextAccessor hca,
        IPaymentIntentPersistence paymentIntentPersistence,
        ISession session,
        ISiteService siteService,
        IStripePaymentService stripePaymentService)
    {
        _hca = hca;
        _paymentIntentPersistence = paymentIntentPersistence;
        _session = session;
        _siteService = siteService;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model)
    {
        PaymentIntent paymentIntent;

        try
        {
            paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(model.SingleCurrencyTotal);
        }
        catch (StripeException exception) when (exception.Message.StartsWithOrdinal("No API key provided."))
        {
            return null;
        }

        if (_hca.HttpContext?.GetRouteValue("action")?.ToString() == nameof(PaymentController.PaymentRequest))
        {
            _session.Save(new OrderPayment
            {
                OrderId = model.OrderPart.ContentItem.ContentItemId,
                PaymentIntentId = paymentIntent.Id,
            });
        }

        return new
        {
            (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            paymentIntent.ClientSecret,
        };
    }

    public Task ValidateAsync(IUpdateModelAccessor updateModelAccessor) =>
        _stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(updateModelAccessor);

    public Task FinalModificationOfOrderAsync(ContentItem order, string shoppingCartId)
    {
        // A new payment intent should be created on the next checkout.
        _paymentIntentPersistence.Remove();

        return Task.CompletedTask;
    }

    public Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(
        Controller controller,
        ContentItem order,
        string shoppingCartId) =>
        throw new NotSupportedException(
            "This code should never be reached, because Stripe payment uses ~/checkout/middleware/Stripe, not ~/checkout/callback/Stripe.");
}
