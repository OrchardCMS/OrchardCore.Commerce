using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Mvc.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;

[DevelopmentAndLocalhostOnly]
public class OrderController : Controller
{
    private readonly Lazy<IPaymentService> _checkoutService;
    private readonly Lazy<IShoppingCartPersistence> _shoppingCartPersistence;
    private readonly Lazy<IContentManager> _contentManager;
    private readonly Lazy<IStripePaymentService> _stripePaymentService;

    public OrderController(
        Lazy<IPaymentService> checkoutService,
        Lazy<IShoppingCartPersistence> shoppingCartPersistence,
        Lazy<IContentManager> contentManager,
        Lazy<IStripePaymentService> stripePaymentService)
    {
        _checkoutService = checkoutService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _contentManager = contentManager;
        _stripePaymentService = stripePaymentService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> CreateOrderWithSuccessfulPayment(long dateTimeTicks)
    {
        var testTime = new DateTime(dateTimeTicks);

        var shoppingCart = await _shoppingCartPersistence.Value.RetrieveAsync();
        var checkoutViewModel = await _checkoutService.Value.CreateCheckoutViewModelAsync(shoppingCart.Id);
        var order = await _contentManager.Value.NewAsync(Order);
        var orderLineItemList = await _stripePaymentService.Value.CreateOrderLineItemListAsync(shoppingCart);
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

            orderPart.LineItems.AddRange(orderLineItemList);

            orderPart.BillingAddress = new AddressField
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
            orderPart.BillingAndShippingAddressesMatch.Value = true;
            orderPart.ShippingAddress = new AddressField
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

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _contentManager.Value.CreateAsync(order);

        await _checkoutService.Value.FinalModificationOfOrderAsync(order);

        return RedirectToAction(
            nameof(PaymentController.Success),
            typeof(PaymentController).ControllerName(),
            new { area = "OrchardCore.Commerce", orderId = order.ContentItemId, });
    }
}
