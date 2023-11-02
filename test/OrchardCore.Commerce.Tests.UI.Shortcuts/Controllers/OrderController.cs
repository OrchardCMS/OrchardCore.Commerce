using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Mvc.Core.Utilities;
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
    private readonly IShoppingCartHelpers _shoppingCartHelpers;

    public OrderController(
        IPaymentService paymentService,
        IShoppingCartPersistence shoppingCartPersistence,
        IContentManager contentManager,
        IShoppingCartHelpers shoppingCartHelpers)
    {
        _paymentService = paymentService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _contentManager = contentManager;
        _shoppingCartHelpers = shoppingCartHelpers;
    }

    [AllowAnonymous]
    public async Task<IActionResult> CreateOrderWithSuccessfulPayment(long dateTimeTicks, string shoppingCartId)
    {
        var testTime = new DateTime(dateTimeTicks, DateTimeKind.Utc);

        var cart = await _shoppingCartPersistence.RetrieveAsync(shoppingCartId: null);
        var checkoutViewModel = await _paymentService.CreateCheckoutViewModelAsync(cart.Id);
        var order = await _contentManager.NewAsync(Order);
        var orderLineItems = await _shoppingCartHelpers.CreateOrderLineItemsAsync(cart);
        order.Apply(checkoutViewModel.OrderPart);

        order.Alter<OrderPart>(orderPart =>
        {
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
        });

        await _contentManager.CreateAsync(order);

        await _paymentService.UpdateOrderToOrderedAsync(order, shoppingCartId, _ => new[]
        {
            new Models.Payment(
                Kind: "Card",
                ChargeText: "Test charge text",
                TransactionId: "Test transaction ID",
                Amount: checkoutViewModel.SingleCurrencyTotal,
                CreatedUtc: testTime),
        });
        await _paymentService.FinalModificationOfOrderAsync(order, shoppingCartId, paymentProviderName: null);

        return RedirectToAction(
            nameof(PaymentController.Success),
            typeof(PaymentController).ControllerName(),
            new { area = "OrchardCore.Commerce.Payment", orderId = order.ContentItemId, });
    }
}
