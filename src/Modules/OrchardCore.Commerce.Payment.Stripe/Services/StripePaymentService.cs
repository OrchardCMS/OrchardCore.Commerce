using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Commerce.Payment.Stripe.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Settings;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using Address = OrchardCore.Commerce.AddressDataType.Address;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IHttpContextAccessor _hca;
    private readonly IContentManager _contentManager;
    private readonly ISiteService _siteService;
    private readonly IStringLocalizer T;
    private readonly YesSql.ISession _session;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly IPaymentService _paymentService;
    private readonly IHtmlLocalizer<StripePaymentService> H;
    private readonly IStripePaymentIntentService _stripePaymentIntentService;

#pragma warning disable S107 // Methods should not have too many parameters
    public StripePaymentService(
        IContentManager contentManager,
        ISiteService siteService,
        IStringLocalizer<StripePaymentService> stringLocalizer,
        YesSql.ISession session,
        IPaymentIntentPersistence paymentIntentPersistence,
        IPaymentService paymentService,
        IHtmlLocalizer<StripePaymentService> htmlLocalizer,
        IHttpContextAccessor hca,
        IStripePaymentIntentService stripePaymentIntentService)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _contentManager = contentManager;
        _siteService = siteService;
        _session = session;
        _paymentIntentPersistence = paymentIntentPersistence;
        T = stringLocalizer;
        _paymentService = paymentService;
        H = htmlLocalizer;
        _hca = hca;
        _stripePaymentIntentService = stripePaymentIntentService;
    }

    public async Task<string> GetPublicKeyAsync()
    {
        var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();
        return stripeApiSettings.PublishableKey;
    }

    public async Task<string> CreateClientSecretAsync(Amount total, ShoppingCartViewModel cart)
    {
        var stripeApiSettings = (await _siteService.GetSiteSettingsAsync()).As<StripeApiSettings>();

        if (string.IsNullOrEmpty(stripeApiSettings.PublishableKey) ||
            string.IsNullOrEmpty(stripeApiSettings.SecretKey) ||
            total.Value <= 0)
        {
            return null;
        }

        var paymentIntentId = await _paymentIntentPersistence.RetrieveAsync(cart.Id);
        var totals = cart.GetTotalsOrThrowIfEmpty();

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
        var defaultTotal = totals.SingleOrDefault();

        var initPaymentIntent = string.IsNullOrEmpty(paymentIntentId)
            ? await _stripePaymentIntentService.CreatePaymentIntentAsync(defaultTotal, cart.Id)
            : await _stripePaymentIntentService.GetOrUpdatePaymentIntentAsync(paymentIntentId, defaultTotal);

        return initPaymentIntent.ClientSecret;
    }

    public async Task UpdateOrderToOrderedAsync(PaymentIntent paymentIntent, string shoppingCartId) =>
        await _paymentService.UpdateOrderToOrderedAsync(
            await GetOrderByPaymentIntentIdAsync(paymentIntent.Id),
            shoppingCartId,
            CreateChargesProvider(paymentIntent));

    public Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
        ContentItem order,
        PaymentIntent paymentIntent,
        string shoppingCartId
        )
    {
        try
        {
            return _paymentService.UpdateAndRedirectToFinishedOrderAsync(
                order,
                shoppingCartId,
                StripePaymentProvider.ProviderName,
                CreateChargesProvider(paymentIntent));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.Failed,
                ShowMessage = H["You have paid the bill, but this system did not record it. Please contact the administrators."],
                HideMessage = ex.Message,
                Content = order,
            });
        }
    }

    private static Func<OrderPart, IEnumerable<Commerce.Abstractions.Models.Payment>> CreateChargesProvider(PaymentIntent paymentIntent) =>
        orderPart => orderPart.Charges.Select(charge => paymentIntent.CreatePayment(charge.Amount));

    public async Task UpdateOrderToPaymentFailedAsync(string paymentIntentId)
    {
        var order = await GetOrderByPaymentIntentIdAsync(paymentIntentId);
        order.Alter<OrderPart>(orderPart =>
            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatusCodes.PaymentFailed });

        await _contentManager.UpdateAsync(order);
    }

    public Task<OrderPayment> GetOrderPaymentByPaymentIntentIdAsync(string paymentIntentId) =>
        _session
            .Query<OrderPayment, OrderPaymentIndex>(index => index.PaymentIntentId == paymentIntentId)
            .FirstOrDefaultAsync();

    public Task SaveOrderPaymentAsync(string orderContentItemId, string paymentIntentId) =>
        _session.SaveAsync(new OrderPayment
        {
            OrderId = orderContentItemId,
            PaymentIntentId = paymentIntentId,
        });

    public async Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(
        IUpdateModelAccessor updateModelAccessor,
        string shoppingCartId,
        string paymentIntentId = null,
        OrderPart orderPart = null)
    {
        var innerPaymentIntentId = paymentIntentId ?? await _paymentIntentPersistence.RetrieveAsync(shoppingCartId);
        var paymentIntent = await _stripePaymentIntentService.GetPaymentIntentAsync(innerPaymentIntentId);

        // Stripe doesn't support multiple shopping cart IDs because we can't send that info to the middleware anyway.
        var (order, isNew) = await _paymentService.CreateOrUpdateOrderFromShoppingCartAsync(
            updateModelAccessor,
            (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntent.Id))?.OrderId,
            shoppingCartId,
            (order, _, total, cartViewModel, lineItems) =>
            {
                order.Alter<OrderPart>(orderPart =>
                {
                    orderPart.Charges.Clear();
                    orderPart.Charges.Add(paymentIntent.CreatePayment(total));

                    if (cartViewModel is null) return;

                    // Shopping cart
                    orderPart.LineItems.SetItems(lineItems);
                    orderPart.Status = new TextField { ContentItem = order, Text = OrderStatusCodes.Pending };

                    // Store the current applicable discount info so they will be available in the future.
                    orderPart.AdditionalData.SetDiscountsByProduct(cartViewModel
                        .Lines
                        .Where(line => line.AdditionalData.GetDiscounts().Any())
                        .ToDictionary(
                            line => line.ProductSku,
                            line => line.AdditionalData.GetDiscounts()));
                });

                order.Alter<StripePaymentPart>(part =>
                    part.PaymentIntentId = new TextField { ContentItem = order, Text = paymentIntent.Id });

                return Task.CompletedTask;
            },
            orderPart);

        if (!order.As<OrderPart>().LineItems.Any() && updateModelAccessor != null)
        {
            updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(OrderPart.LineItems),
                T["The order is empty."]);
        }

        if (isNew)
        {
            await SaveOrderPaymentAsync(order.ContentItemId, paymentIntent.Id);
        }

        return order;
    }

    public async Task<PaymentOperationStatusViewModel> PaymentConfirmationAsync(
        string paymentIntentId,
        string shoppingCartId,
        bool needToJudgeIntentStorage = true)
    {
        // If it is null it means the session was not loaded yet and a redirect is needed.
        if (needToJudgeIntentStorage && string.IsNullOrEmpty(await _paymentIntentPersistence.RetrieveAsync(shoppingCartId)))
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.WaitingForRedirect,
                Url = _hca.HttpContext.Request.GetDisplayUrl(),
            };
        }

        // If we can't find a valid payment intent based on ID or if we can't find the associated order, then something
        // went wrong and continuing from here would only cause a crash anyway.
        if (await _stripePaymentIntentService.GetPaymentIntentAsync(paymentIntentId) is not { PaymentMethod: not null } fetchedPaymentIntent ||
            (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntentId))?.OrderId is not { } orderId ||
            await _contentManager.GetAsync(orderId) is not { } order)
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.NotFound,
                ShowMessage = H[
                    "Couldn't find the payment intent \"{0}\" or the order associated with it.",
                    paymentIntentId ?? string.Empty],
            };
        }

        var part = order.As<OrderPart>() ?? new OrderPart();
        var succeeded = fetchedPaymentIntent.Status == PaymentIntentStatuses.Succeeded;

        // Looks like there is nothing to do here.
        if (succeeded && part.IsOrdered)
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.NotThingToDo,
                Content = order,
            };
        }

        if (succeeded && part.IsPending)
        {
            return await UpdateAndRedirectToFinishedOrderAsync(
                order,
                fetchedPaymentIntent,
                shoppingCartId
                );
        }

        if (part.IsFailed)
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.Failed,
                ShowMessage = H["The payment has failed, please try again."],
            };
        }

        order.Alter<StripePaymentPart>(part =>
        {
            part.PaymentIntentId.Text = fetchedPaymentIntent.Id;
            part.RetryCounter++;
        });
        await _contentManager.UpdateAsync(order);

        if (order.As<StripePaymentPart>().RetryCounter <= 10)
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.WaitingForRedirect,
                Url = _hca.HttpContext.Request.GetDisplayUrl(),
            };
        }

        // Delete payment intent from session, to create a new one.
        await _paymentIntentPersistence.RemoveAsync(shoppingCartId);
        return new PaymentOperationStatusViewModel
        {
            Status = PaymentOperationStatus.Failed,
            ShowMessage = H["The payment has failed, please try again."],
        };
    }

    public async Task<PaymentIntentConfirmOptions> GetStripeConfirmParametersAsync(
        string returnUrl,
        ContentItem order = null)
    {
        if (order == null)
        {
            order = await _contentManager.NewAsync(Commerce.Abstractions.Constants.ContentTypes.Order);
            await _paymentService.UpdateOrderWithDriversAsync(order);
        }

        var part = order.As<OrderPart>();
        var billing = part.BillingAddress.Address ?? new Address();
        var shipping = part.ShippingAddress.Address ?? new Address();

        var model = new PaymentIntentConfirmOptions
        {
            ReturnUrl = returnUrl,
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
            Shipping = string.IsNullOrEmpty(shipping.Name)
                ? null
                : new ChargeShippingOptions
                {
                    Name = shipping.Name,
                    Phone = part.Phone?.Text,
                    Address = CreateAddressOptions(shipping),
                },
        };
        return model;
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

    private async Task<ContentItem> GetOrderByPaymentIntentIdAsync(string paymentIntentId)
    {
        var orderId = (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntentId))?.OrderId;
        return await _contentManager.GetAsync(orderId);
    }
}
