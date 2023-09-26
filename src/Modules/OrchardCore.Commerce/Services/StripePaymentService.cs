using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using OrchardCore.Workflows.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly IContentManager _contentManager;
    private readonly IStringLocalizer T;
    private readonly ISession _session;
    private readonly IPaymentIntentPersistence _paymentIntentPersistence;
    private readonly RequestOptions _requestOptions;
    private readonly string _siteName;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IProductInventoryService _productInventoryService;
    private readonly IEnumerable<IWorkflowManager> _workflowManagers;
    private readonly IPriceSelectionStrategy _priceSelectionStrategy;
    private readonly IPriceService _priceService;
    private readonly IProductService _productService;
    private readonly IMoneyService _moneyService;

    // We need to use that many, this cannot be avoided.
#pragma warning disable S107 // Methods should not have too many parameters
    public StripePaymentService(
        IShoppingCartHelpers shoppingCartHelpers,
        IShoppingCartPersistence shoppingCartPersistence,
        IContentManager contentManager,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<StripePaymentService> logger,
        IStringLocalizer<StripePaymentService> stringLocalizer,
        ISession session,
        IPaymentIntentPersistence paymentIntentPersistence,
        IContentItemDisplayManager contentItemDisplayManager,
        IProductInventoryService productInventoryService,
        IEnumerable<IWorkflowManager> workflowManagers,
        IPriceSelectionStrategy priceSelectionStrategy,
        IPriceService priceService,
        IProductService productService,
        IMoneyService moneyService)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _paymentIntentService = new PaymentIntentService();
        _shoppingCartHelpers = shoppingCartHelpers;
        _shoppingCartPersistence = shoppingCartPersistence;
        _contentManager = contentManager;
        _session = session;
        _paymentIntentPersistence = paymentIntentPersistence;
        _productInventoryService = productInventoryService;
        _priceSelectionStrategy = priceSelectionStrategy;
        _priceService = priceService;
        _productService = productService;
        _moneyService = moneyService;
        T = stringLocalizer;
        _contentItemDisplayManager = contentItemDisplayManager;
        _workflowManagers = workflowManagers;

        var siteSettings = siteService.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult();
        _siteName = siteSettings.SiteName;
        _requestOptions =
            new RequestOptions
            {
                ApiKey = siteSettings
                    .As<StripeApiSettings>()
                    .SecretKey
                    .DecryptStripeApiKey(dataProtectionProvider, logger),
            };
    }

    public async Task<PaymentIntent> InitializePaymentIntentAsync(string paymentIntentId)
    {
        var orderPart = (await GetOrderByPaymentIntentIdAsync(paymentIntentId))?.As<OrderPart>();
        var totals = CheckTotals(
            await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
                shoppingCartId: null,
                orderPart?.ShippingAddress.Address,
                orderPart?.BillingAddress.Address));

        // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
        // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
        var defaultTotal = totals.SingleOrDefault();

        var currencyType = defaultTotal.Currency.CurrencyIsoCode;
        long amountForPayment = GetPaymentAmount(defaultTotal.Value, currencyType);

        return string.IsNullOrEmpty(paymentIntentId)
            ? await CreatePaymentIntentAsync(amountForPayment, defaultTotal)
            : await GetOrUpdatePaymentIntentAsync(paymentIntentId, amountForPayment, defaultTotal);
    }

    public Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentGetOptions = new PaymentIntentGetOptions();
        paymentIntentGetOptions.AddExpansions();
        return _paymentIntentService.GetAsync(
            paymentIntentId,
            paymentIntentGetOptions,
            _requestOptions.SetIdempotencyKey());
    }

    public async Task UpdateOrderToOrderedAsync(PaymentIntent paymentIntent = null, ContentItem orderItem = null)
    {
        var order = paymentIntent != null
            ? await GetOrderByPaymentIntentIdAsync(paymentIntent.Id)
            : orderItem;

        order.Alter<OrderPart>(orderPart =>
        {
            if (paymentIntent != null)
            {
                // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
                // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
                var orderPartCharge = orderPart.Charges.SingleOrDefault();
                var amount = orderPartCharge!.Amount;

                var payment = new Payment
                {
                    Kind = paymentIntent.PaymentMethod.GetFormattedPaymentType(),
                    ChargeText = paymentIntent.Description,
                    TransactionId = paymentIntent.Id,
                    Amount = amount,
                    CreatedUtc = paymentIntent.Created,
                };

                orderPart.Charges.Clear();
                orderPart.Charges.Add(payment);
            }

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _workflowManagers.TriggerContentItemEventAsync<OrderCreatedEvent>(order);

        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();

        // Decrease inventories of purchased items.
        await _productInventoryService.UpdateInventoriesAsync(currentShoppingCart.Items);

        await _contentManager.UpdateAsync(order);
    }

    public async Task UpdateOrderToPaymentFailedAsync(PaymentIntent paymentIntent)
    {
        var order = await GetOrderByPaymentIntentIdAsync(paymentIntent.Id);
        order.Alter<OrderPart>(orderPart =>
            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.PaymentFailed.HtmlClassify() });

        await _contentManager.UpdateAsync(order);
    }

    public Task<OrderPayment> GetOrderPaymentByPaymentIntentIdAsync(string paymentIntentId) =>
        _session
            .Query<OrderPayment, OrderPaymentIndex>(index => index.PaymentIntentId == paymentIntentId)
            .FirstOrDefaultAsync();

    public async Task<ContentItem> CreateOrUpdateOrderFromShoppingCartAsync(
        PaymentIntent paymentIntent,
        IUpdateModelAccessor updateModelAccessor)
    {
        var currentShoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var orderId = (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntent.Id))?.OrderId;

        if (!string.IsNullOrEmpty(orderId) && await _contentManager.GetAsync(orderId) is { } order)
        {
            // If there are line items in the Order, use data from Order instead of shopping cart.
            if (!order.As<OrderPart>().LineItems.Any() &&
                currentShoppingCart.Items.Any() &&
                await UpdateOrderWithDriversAsync(order, updateModelAccessor))
            {
                return null;
            }
        }
        else
        {
            order = await _contentManager.NewAsync(Constants.ContentTypes.Order);
            if (await UpdateOrderWithDriversAsync(order, updateModelAccessor))
            {
                return null;
            }

            _session.Save(new OrderPayment
            {
                OrderId = order.ContentItemId,
                PaymentIntentId = paymentIntent.Id,
            });
        }

        var orderPart = order.As<OrderPart>();

        var lineItems = await CreateOrderLineItemsAsync(currentShoppingCart);
        if (!lineItems.Any())
        {
            lineItems = orderPart.LineItems;
        }

        var cartViewModel = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(
            shoppingCartId: null,
            orderPart.ShippingAddress.Address,
            orderPart.BillingAddress.Address);

        var currency = orderPart.LineItems.Any() ? orderPart.LineItems[0].LinePrice.Currency : _moneyService.DefaultCurrency;
        var orderTotals = new Amount(0, currency);

        if (cartViewModel is null)
        {
            orderTotals = CalculateOrderTotals(orderTotals, orderPart);
        }

        // If there is no cart, use current Order's data.
        var defaultTotal = cartViewModel is null ? orderTotals : CheckTotals(cartViewModel).SingleOrDefault();
        AlterOrder(order, paymentIntent, defaultTotal, cartViewModel, lineItems);

        if (string.IsNullOrEmpty(orderId))
        {
            await _contentManager.CreateAsync(order);
        }
        else
        {
            await _contentManager.UpdateAsync(order);
        }

        return order;
    }

    public long GetPaymentAmount(decimal defaultTotalValue, string currencyType)
    {
        long amountForPayment;

        // If I convert it to conditional expression, it will warn me to extract it again.
#pragma warning disable IDE0045 // Convert to conditional expression
        // We need to convert the value (decimal) to long.
        // https://stripe.com/docs/currencies#zero-decimal
        if (CurrencyCollectionConstants.ZeroDecimalCurrencies.Contains(currencyType))
        {
            amountForPayment = (long)Math.Round(defaultTotalValue);
        }
        else if (CurrencyCollectionConstants.SpecialCases.Contains(currencyType))
        {
            amountForPayment = (long)Math.Round(defaultTotalValue / 100m) * 10000;
        }
        else
        {
            amountForPayment = (long)Math.Round(defaultTotalValue * 100);
        }

        return amountForPayment;
    }

    private async Task<bool> UpdateOrderWithDriversAsync(ContentItem order, IUpdateModelAccessor updateModelAccessor)
    {
        await _contentItemDisplayManager.UpdateEditorAsync(order, updateModelAccessor.ModelUpdater, isNew: false);

        return updateModelAccessor.ModelUpdater.GetModelErrorMessages().Any();
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(long amountForPayment, Amount defaultTotal)
    {
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = amountForPayment,
            Currency = defaultTotal.Currency.CurrencyIsoCode,
            Description = T["User checkout on {0}", _siteName].Value,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true, },
        };

        var paymentIntent = await _paymentIntentService.CreateAsync(
            paymentIntentOptions,
            _requestOptions.SetIdempotencyKey());

        _paymentIntentPersistence.Store(paymentIntent.Id);

        return paymentIntent;
    }

    public async Task<IEnumerable<OrderLineItem>> CreateOrderLineItemsAsync(ShoppingCart shoppingCart)
    {
        var lineItems = new List<OrderLineItem>();

        // This needs to be done separately because it's async: "Avoid using async lambda when delegate type returns
        // void."
        foreach (var item in shoppingCart.Items)
        {
            var trimmedSku = item.ProductSku.Split('-')[0];

            var contentItemId = (await _session
                    .QueryIndex<ProductPartIndex>(productPartIndex => productPartIndex.Sku == trimmedSku)
                    .ListAsync())
                .Select(index => index.ContentItemId)
                .First();

            var contentItemVersion = (await _contentManager.GetAsync(contentItemId)).ContentItemVersionId;

            lineItems.Add(await item.CreateOrderLineFromShoppingCartItemAsync(
                _priceSelectionStrategy,
                _priceService,
                _productService,
                contentItemVersion));
        }

        return lineItems;
    }

    private static void AlterOrder(
        ContentItem order,
        PaymentIntent paymentIntent,
        Amount defaultTotal,
        ShoppingCartViewModel cartViewModel,
        IEnumerable<OrderLineItem> lineItems)
    {
        order.Alter<OrderPart>(orderPart =>
        {
            orderPart.Charges.Clear();
            orderPart.Charges.Add(
                new Payment
                {
                    ChargeText = paymentIntent.Description,
                    TransactionId = paymentIntent.Id,
                    Amount = defaultTotal,
                    CreatedUtc = paymentIntent.Created,
                });

            if (cartViewModel is not null)
            {
                // Shopping cart
                orderPart.LineItems.SetItems(lineItems);
                orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Pending.HtmlClassify() };

                // Store the current applicable discount info so they will be available in the future.
                orderPart.AdditionalData.SetDiscountsByProduct(cartViewModel
                    .Lines
                    .Where(line => line.AdditionalData.GetDiscounts().Any())
                    .ToDictionary(
                        line => line.ProductSku,
                        line => line.AdditionalData.GetDiscounts()));
            }
        });

        order.Alter<StripePaymentPart>(part => part.PaymentIntentId = new TextField { ContentItem = order, Text = paymentIntent.Id });
    }

    private static Amount CalculateOrderTotals(Amount orderTotals, OrderPart orderPart)
    {
        foreach (var item in orderPart.LineItems)
        {
            orderTotals += item.LinePrice;
        }

        return orderTotals;
    }

    private async Task<PaymentIntent> GetOrUpdatePaymentIntentAsync(
        string paymentIntentId,
        long amountForPayment,
        Amount defaultTotal)
    {
        var paymentIntent = await GetPaymentIntentAsync(paymentIntentId);

        if (paymentIntent?.Status is PaymentIntentStatuses.Succeeded or PaymentIntentStatuses.Processing)
        {
            return paymentIntent;
        }

        return await UpdatePaymentIntentAsync(paymentIntentId, amountForPayment, defaultTotal);
    }

    private Task<PaymentIntent> UpdatePaymentIntentAsync(
        string paymentIntentId,
        long amountForPayment,
        Amount defaultTotal)
    {
        var updateOptions = new PaymentIntentUpdateOptions
        {
            Amount = amountForPayment,
            Currency = defaultTotal.Currency.CurrencyIsoCode,
        };

        updateOptions.AddExpansions();
        return _paymentIntentService.UpdateAsync(
            paymentIntentId,
            updateOptions,
            _requestOptions.SetIdempotencyKey());
    }

    private static IList<Amount> CheckTotals(ShoppingCartViewModel viewModel)
    {
        if (!viewModel.Totals.Any())
        {
            throw new InvalidOperationException("Cannot create a payment without shopping cart total(s)!");
        }

        return viewModel.Totals;
    }

    private async Task<ContentItem> GetOrderByPaymentIntentIdAsync(string paymentIntentId)
    {
        var orderId = (await GetOrderPaymentByPaymentIntentIdAsync(paymentIntentId))?.OrderId;
        return await _contentManager.GetAsync(orderId);
    }
}
