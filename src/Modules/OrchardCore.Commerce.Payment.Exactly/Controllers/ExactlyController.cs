using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Lombiq.HelpfulLibraries.OrchardCore.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Exactly.Drivers;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Commerce.Payment.Exactly.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using Refit;
using System;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

using AdminController = OrchardCore.Settings.Controllers.AdminController;
using FrontendException = Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

namespace OrchardCore.Commerce.Payment.Exactly.Controllers;

public class ExactlyController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IExactlyService _exactlyService;
    private readonly ILogger<ExactlyController> _logger;
    private readonly INotifier _notifier;
    private readonly IPaymentService _paymentService;
    private readonly IHtmlLocalizer<ExactlyController> H;

    public ExactlyController(
        IExactlyService exactlyService,
        INotifier notifier,
        IPaymentService paymentService,
        IOrchardServices<ExactlyController> services)
    {
        _contentManager = services.ContentManager.Value;
        _exactlyService = exactlyService;
        _logger = services.Logger.Value;
        _notifier = notifier;
        _paymentService = paymentService;
        H = services.HtmlLocalizer.Value;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransaction(string shoppingCartId) =>
        await this.SafeJsonAsync(async () =>
        {
            var order = await _paymentService.CreatePendingOrderFromShoppingCartAsync(
                shoppingCartId,
                notifyOnError: false,
                throwOnError: true);
            return await _exactlyService.CreateTransactionAsync(order.As<OrderPart>());
        });

    public async Task<IActionResult> GetRedirectUrl(string transactionId) =>
        await this.SafeJsonAsync<object>(async () => await GetActionRedirectRequestedAsync(transactionId));

    public async Task<IActionResult> VerifyApi()
    {
        try
        {
            var order = await _contentManager.NewAsync(Order);
            order.Alter<OrderPart>(part =>
            {
                part.OrderId.Text = Guid.NewGuid().ToString("D");
                part.LineItems.Add(new OrderLineItem(
                    quantity: 1,
                    "TEST",
                    "TEST",
                    new Amount(1, Currency.Euro),
                    new Amount(1, Currency.Euro),
                    contentItemVersion: null));
            });
            await _contentManager.CreateAsync(order);

            var result = await _exactlyService.CreateTransactionAsync(order.As<OrderPart>());
            var action = await GetActionRedirectRequestedAsync(result.Id);

            await _notifier.SuccessAsync(
                H["The Exactly API access works correctly. You can test the redirection by clicking <a href=\"{0}\">here</a>", action.Url]);
        }
        catch (ApiException exception)
        {
            _logger.LogError(exception, "An API error was encountered.");
            await _notifier.ErrorAsync(H["An API error was encountered: {0}", exception.Message]);
        }
        catch (FrontendException exception)
        {
            _logger.LogError(exception, "A front-end readable error was encountered.");
            await _notifier.FrontEndErrorAsync(exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unknown error was encountered.");

            var error = exception.ToString();
            var html = $"{H["An unknown error was encountered:"].Html()}<br>{error.Replace("\n", "<br>")}";
            await _notifier.ErrorAsync(new LocalizedHtmlString(html, html));
        }

        return RedirectToAction(
            nameof(AdminController.Index),
            typeof(AdminController).ControllerName(),
            new { area = "OrchardCore.Settings", groupId = ExactlySettingsDisplayDriver.EditorGroupId });
    }

    private async Task<ChargeAction.ChargeActionAttributes> GetActionRedirectRequestedAsync(string transactionId)
    {
        var result = await _exactlyService.GetTransactionDetailsAsync(
            transactionId,
            ChargeResponse.ChargeResponseStatus.ActionRequired,
            HttpContext.RequestAborted);
        return result
            .Attributes
            .Actions
            .Select(action => action.Attributes)
            .FirstOrDefault(action => action.Action == "redirect-required" && action.IsGet);
    }
}
