using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Abstractions.Exceptions;
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
using System.Text.Json;
using System.Threading.Tasks;
using AdminController=OrchardCore.Settings.Controllers.AdminController;

namespace OrchardCore.Commerce.Payment.Exactly.Controllers;

public class ExactlyController : Controller
{
    private readonly IExactlyApi _api;
    private readonly ILogger<ExactlyController> _logger;
    private readonly INotifier _notifier;
    private readonly IPaymentService _paymentService;
    private readonly IHtmlLocalizer<ExactlyController> H;
    private readonly IStringLocalizer<ExactlyController> S;

    public ExactlyController(
        IExactlyApi api,
        ILogger<ExactlyController> logger,
        INotifier notifier,
        IPaymentService paymentService,
        IHtmlLocalizer<ExactlyController> htmlLocalizer,
        IStringLocalizer<ExactlyController> stringLocalizer)
    {
        _api = api;
        _logger = logger;
        _notifier = notifier;
        _paymentService = paymentService;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(string shoppingCartId) =>
        await this.SafeJsonAsync<object>(async () =>
        {
            try
            {
                var order = await _paymentService.CreatePendingOrderFromShoppingCartAsync(
                    shoppingCartId,
                    notifyOnError: false,
                    throwOnError: true);
                var request = await ChargeRequest.CreateUserAsync(order!.As<OrderPart>(), HttpContext);

                return ThrowIfError(await _api.CreateTransactionAsync(request));
            }
            catch (FrontendException exception)
            {
                return new { error = exception.Message, html = exception.HtmlMessage.Html() };
            }
        });

    public async Task<IActionResult> GetRedirectUrl(string transactionId) =>
        await this.SafeJsonAsync<object>(async () =>
        {
            try
            {
                var content = ThrowIfError(await _api.GetTransactionDetailsAsync(transactionId));
                return content.Attributes.Actions.First(action => action.Action != null).Action;
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
            var orderPart = new OrderPart
            {
                OrderId =
                {
                    Text = Guid.NewGuid().ToString("D"),
                },
                Charges =
                {
                    new Payment.Models.Payment(
                        "card",
                        Guid.NewGuid().ToString("D"),
                        "Test Transaction",
                        new Amount(0, Currency.Euro), DateTime.UtcNow),
                },
            };

            var charge = await ChargeRequest.CreateUserAsync(orderPart, HttpContext);
            using var result = await _api.CreateTransactionAsync(new ExactlyRequest<ChargeRequest> { Attributes = charge });
            await result.EnsureSuccessStatusCodeAsync();
            result.Content!.ThrowIfHasErrors();

            await _notifier.SuccessAsync(H["The Exactly API access works correctly ({0}).", JsonSerializer.Serialize(result.Content.Data)]);
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
            await _notifier.ErrorAsync(H["An unknown error was encountered: {0}", exception.ToString()]);
        }

        return RedirectToAction(
            nameof(AdminController.Index),
            typeof(AdminController).ControllerName(),
            new { area = "OrchardCore.Settings", groupId = ExactlySettingsDisplayDriver.EditorGroupId });
    }

    private T ThrowIfError<T>(ApiResponse<ExactlyResponse<T>> response)
        where T : IExactlyResponseData
    {
        using (response)
        {
            response.Content?.ThrowIfHasErrors();
            if (response.Error is { } error) throw new FrontendException(error.Message);
            if (response.Content is not { } content)
            {
                throw new FrontendException(S["There was an unknown error while contacting with the payment service, please try again."]);
            }

            return content.Data;
        }
    }
}
