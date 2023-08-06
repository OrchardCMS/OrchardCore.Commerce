using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;

[DevelopmentAndLocalhostOnly]
public class OrderController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IWorkflowManager> _workflowManagers;
    private readonly IStripePaymentService _stripePaymentService;

    public OrderController(
        IPaymentService paymentService,
        IShoppingCartPersistence shoppingCartPersistence,
        IContentManager contentManager,
        IEnumerable<IWorkflowManager> workflowManagers,
        IStripePaymentService stripePaymentService)
    {
        _paymentService = paymentService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _contentManager = contentManager;
        _workflowManagers = workflowManagers;
        _stripePaymentService = stripePaymentService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> CreateOrderWithSuccessfulPayment(long dateTimeTicks)
    {
        var testTime = new DateTime(dateTimeTicks);

        var shoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var checkoutViewModel = await _paymentService.CreateCheckoutViewModelAsync(shoppingCart.Id);
        var order = await _contentManager.NewAsync(Order);
        var orderLineItems = await _stripePaymentService.CreateOrderLineItemsAsync(shoppingCart);
        order.Apply(checkoutViewModel.OrderPart);

        order.Alter<OrderPart>(orderPart =>
        {
            var payment = new Payment
            {
                Kind = "Card",
                ChargeText = "Test charge text",
                TransactionId = "Test transaction ID",
                Amount = checkoutViewModel.SingleCurrencyTotal,
                CreatedUtc = testTime,
            };

            orderPart.Charges.Clear();
            orderPart.Charges.Add(payment);

            orderPart.LineItems.AddRange(orderLineItems);
            var addressField = new AddressField
            {
                Address = new Address
                {
                    City = "TestCity",
                    Company = "TestCompany",
                    Department = "TestDepartment",
                    Name = "TestName",
                    Province = "TestProvince",
                    Region = "TestRegion",
                    PostalCode = "TestPostalCode",
                    StreetAddress1 = "TestStreetAddress1",
                    StreetAddress2 = "TestStreetAddress2",
                },
            };

            orderPart.BillingAddress = addressField;
            orderPart.BillingAndShippingAddressesMatch.Value = true;
            orderPart.ShippingAddress = addressField;

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _contentManager.CreateAsync(order);

        await _paymentService.FinalModificationOfOrderAsync(order);

        // Since the event trigger is tied to "UpdateOrderToOrderedAsync()" we also need to call it here.
        await _workflowManagers.TriggerContentItemEventAsync<OrderCreatedEvent>(order);

        return RedirectToAction(
            nameof(PaymentController.Success),
            typeof(PaymentController).ControllerName(),
            new { area = "OrchardCore.Commerce", orderId = order.ContentItemId, });
    }
}
