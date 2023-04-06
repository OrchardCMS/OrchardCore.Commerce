using Lombiq.HelpfulLibraries.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;

[DevelopmentAndLocalhostOnly]
public class OrderController : Controller
{
    private readonly Lazy<ICheckoutService> _checkoutService;
    private readonly Lazy<IShoppingCartPersistence> _shoppingCartPersistence;
    private readonly IClock _clock;
    private readonly Lazy<IContentManager> _contentManager;

    public OrderController(
        Lazy<ICheckoutService> checkoutService,
        Lazy<IShoppingCartPersistence> shoppingCartPersistence,
        IClock clock,
        Lazy<IContentManager> contentManager)
    {
        _checkoutService = checkoutService;
        _shoppingCartPersistence = shoppingCartPersistence;
        _clock = clock;
        _contentManager = contentManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> CreateOrderWithSuccessfulPayment()
    {
        var testTime = _clock.UtcNow;

        var shoppingCart = await _shoppingCartPersistence.Value.RetrieveAsync();
        var checkoutViewModel = await _checkoutService.Value.CreateCheckoutViewModelAsync(shoppingCart.Id);
        var order = await _contentManager.Value.NewAsync(Order);
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

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _contentManager.Value.CreateAsync(order);

        await _checkoutService.Value.FinalModificationOfOrderAsync(order);

        return RedirectToAction(
            nameof(PaymentController.Success),
            typeof(PaymentController).ControllerName(),
            new
            {
                area = "OrchardCore.Commerce",
                orderId = order.ContentItemId,
            });
    }
}
