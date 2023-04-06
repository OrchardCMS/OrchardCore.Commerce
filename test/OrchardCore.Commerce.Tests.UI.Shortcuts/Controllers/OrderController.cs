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
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.UI.Shortcuts.Controllers;

[DevelopmentAndLocalhostOnly]
public class OrderController : Controller
{
    private readonly ICheckoutService _checkoutService;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    private readonly IClock _clock;
    private readonly IContentManager _contentManager;

    public OrderController(
        ICheckoutService checkoutService,
        IShoppingCartPersistence shoppingCartPersistence,
        IClock clock,
        IContentManager contentManager)
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

        var shoppingCart = await _shoppingCartPersistence.RetrieveAsync();
        var checkoutViewModel = await _checkoutService.CreateCheckoutViewModelAsync(shoppingCart.Id);
        var order = checkoutViewModel.OrderPart.ContentItem;
        order.Alter<OrderPart>(orderPart =>
        {
            // Same here as on the checkout page: Later we have to figure out what to do if there are multiple
            // totals i.e., multiple currencies. https://github.com/OrchardCMS/OrchardCore.Commerce/issues/132
            var orderPartCharge = orderPart.Charges.SingleOrDefault();
            var amount = orderPartCharge!.Amount;

            var payment = new Payment
            {
                Kind = "Card",
                ChargeText = "Test charge text",
                TransactionId = "Test transaction ID",
                Amount = amount,
                CreatedUtc = testTime,
            };

            orderPart.Charges.Clear();
            orderPart.Charges.Add(payment);

            orderPart.Status = new TextField { ContentItem = order, Text = OrderStatuses.Ordered.HtmlClassify() };
        });

        await _contentManager.UpdateAsync(order);

        await _checkoutService.FinalModificationOfOrderAsync(order);

        return RedirectToAction(
            nameof(PaymentController.Success),
            typeof(PaymentController).ControllerName(),
            new
            {
                area = "OrchardCore.Commerce",
                order.ContentItemId,
            });
    }
}
