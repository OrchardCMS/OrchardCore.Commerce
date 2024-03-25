using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions.Exceptions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Exactly.Drivers;
using OrchardCore.Commerce.Payment.Exactly.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using Refit;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AdminController = OrchardCore.Settings.Controllers.AdminController;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Payment.Exactly.Controllers;

public class ExactlyController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IExactlyService _exactlyService;
    private readonly ILogger<ExactlyController> _logger;
    private readonly INotifier _notifier;
    private readonly IPaymentService _paymentService;
    private readonly IHtmlLocalizer<ExactlyController> H;
    private readonly IStringLocalizer<ExactlyController> S;

    public ExactlyController(
        IContentManager contentManager,
        IExactlyService exactlyService,
        ILogger<ExactlyController> logger,
        INotifier notifier,
        IPaymentService paymentService,
        IHtmlLocalizer<ExactlyController> htmlLocalizer,
        IStringLocalizer<ExactlyController> stringLocalizer)
    {
        _contentManager = contentManager;
        _exactlyService = exactlyService;
        _logger = logger;
        _notifier = notifier;
        _paymentService = paymentService;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(string shoppingCartId) =>
        await this.SafeJsonAsync(async () =>
        {
            try
            {
                var order = await _paymentService.CreatePendingOrderFromShoppingCartAsync(
                    shoppingCartId,
                    notifyOnError: false,
                    throwOnError: true);
                return await _exactlyService.CreateTransactionAsync(order.As<OrderPart>());
            }
            catch (Exception exception)
            {
                return Error(exception);
            }
        });

    public async Task<IActionResult> GetRedirectUrl(string transactionId) =>
        await this.SafeJsonAsync<object>(async () =>
        {
            try
            {
                var result = await _exactlyService.GetTransactionDetailsAsync(transactionId);
                return result.Attributes.Actions.First(action => action.Action != null).Action;
            }
            catch (FrontendException exception)
            {
                return new { error = exception.Message, html = exception.HtmlMessage.Html() };
            }
        });

    [HttpGet("checkout/middleware/Exactly")]
    public async Task<IActionResult> Middleware()
    {
        throw new NotSupportedException("TODO");
    }

    public async Task<IActionResult> VerifyApi()
    {
        try
        {
            var order = await _contentManager.NewAsync(Order);
            order.Alter<OrderPart>(part =>
            {
                part.OrderId.Text = Guid.NewGuid().ToString("D");
                part.Charges.Add(new Payment.Models.Payment(
                    "card",
                    Guid.NewGuid().ToString("D"),
                    "Test Transaction",
                    new Amount(1, Currency.Euro),
                    DateTime.UtcNow));
            });

            var result = await _exactlyService.CreateTransactionAsync(order.As<OrderPart>());
            await _notifier.SuccessAsync(H["The Exactly API access works correctly ({0}).", JsonSerializer.Serialize(result)]);
        }
        catch (ApiException exception)
        {
            _logger.LogError(exception, "An API error was encountered.");
            await _notifier.ErrorAsync(H["An API error was encountered: {0}", exception.Message]);
        }
        catch (FrontendException exception)
        {
            _logger.LogError(exception, "A front-end readable error was encountered.");
            await _notifier.ErrorAsync(exception.HtmlMessage);
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

    private object Error(Exception exception)
    {
        if (exception is FrontendException frontendException)
        {
            return Error(exception.Message, frontendException.HtmlMessage.Html());
        }

        return Error(HttpContext.IsDevelopmentAndLocalhost()
            ? exception.Message
            : S["There was an unknown error while contacting with the payment service, please try again."]);
    }

    private object Error(string text, string html = null) =>
        new { error = text, html = html ?? new HtmlContentString(text).Html() };
}
