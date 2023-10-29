using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Threading.Tasks;
using ISession=YesSql.ISession;

namespace OrchardCore.Commerce.Services;

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
        var paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(model.SingleCurrencyTotal);

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
            StripePublishableKey = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>().PublishableKey,
            PaymentIntentClientSecret = paymentIntent.ClientSecret,
        };
    }

    public Task ValidateAsync(IUpdateModelAccessor updateModelAccessor) =>
        _stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(updateModelAccessor);

    public Task FinalModificationOfOrderAsync(ContentItem order, string shoppingCartId)
    {
        // Set back to default, because a new payment intent should be created on the next checkout.
        _paymentIntentPersistence.Store(paymentIntentId: string.Empty);

        return Task.CompletedTask;
    }
}
