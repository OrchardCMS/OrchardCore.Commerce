using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Exceptions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Commerce.Payment.Exactly.Services;
using OrchardCore.ContentManagement;
using Refit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Controllers;

public class ExactlyController : Controller
{
    private readonly IExactlyApi _api;
    private readonly IPaymentService _paymentService;
    private readonly IStringLocalizer<ExactlyController> H;

    public ExactlyController(IExactlyApi api, IPaymentService paymentService, IStringLocalizer<ExactlyController> stringLocalizer)
    {
        _api = api;
        _paymentService = paymentService;
        H = stringLocalizer;
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

    private T ThrowIfError<T>(ApiResponse<ExactlyResponse<T>> response)
        where T : IExactlyResponseData
    {
        using (response)
        {
            response.Content?.ThrowIfHasErrors();
            if (response.Error is { } error) throw new FrontendException(error.Message);
            if (response.Content is not { } content)
            {
                throw new FrontendException(H["There was an unknown error while contacting with the payment service, please try again."]);
            }

            return content.Data;
        }
    }
}
