using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;

[DevelopmentAndLocalhostOnly]
public class OrderController : PaymentBaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IContentManager _contentManager;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;
    private readonly IHtmlLocalizer _htmlLocalizer;
    public OrderController(
        IPaymentService paymentService,
        IShoppingCartPersistence shoppingCartPersistence,
        IContentManager contentManager,
        IShoppingCartHelpers shoppingCartHelpers,
        IHtmlLocalizer htmlLocalizer,
        INotifier notifier)
        : base(notifier)
    {
        _paymentService = paymentService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _contentManager = contentManager;
        _shoppingCartHelpers = shoppingCartHelpers;
        _htmlLocalizer = htmlLocalizer;
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

        var result = await _paymentService.UpdateAndRedirectToFinishedOrderAsync(
            order,
            shoppingCartId,
            _htmlLocalizer,
            getCharges: _ => new[]
            {
                new Payment.Models.Payment(
                    Kind: "Card",
                    ChargeText: "Test charge text",
                    TransactionId: "Test transaction ID",
                    Amount: checkoutViewModel.SingleCurrencyTotal,
                    CreatedUtc: testTime),
            });
        return await ProduceResultAsync(result);
    }
}
