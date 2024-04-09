using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Exactly";

    private readonly IStringLocalizer<ChargeResponse> _chargeResponseStringLocalizer;
    private readonly IExactlyService _exactlyService;
    private readonly IHttpContextAccessor _hca;
    private readonly IMoneyService _moneyService;
    private readonly INotifier _notifier;
    private readonly IPaymentService _paymentService;
    private readonly ISiteService _siteService;
    private readonly IHtmlLocalizer<ExactlyPaymentProvider> H;

    public string Name => ProviderName;

    public ExactlyPaymentProvider(
        IStringLocalizer<ChargeResponse> chargeResponseStringLocalizer,
        IExactlyService exactlyService,
        IMoneyService moneyService,
        INotifier notifier,
        IPaymentService paymentService,
        IOrchardServices<ExactlyPaymentProvider> services)
    {
        _chargeResponseStringLocalizer = chargeResponseStringLocalizer;
        _exactlyService = exactlyService;
        _hca = services.HttpContextAccessor.Value;
        _moneyService = moneyService;
        _notifier = notifier;
        _paymentService = paymentService;
        _siteService = services.SiteService.Value;
        H = services.HtmlLocalizer.Value;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model)
    {
        var settings = (await _siteService.GetSiteSettingsAsync())?.As<ExactlySettings>();
        return string.IsNullOrEmpty(settings?.ApiKey) || string.IsNullOrEmpty(settings.ProjectId) ? null : new object();
    }

    public async Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(
        Controller controller,
        ContentItem order,
        string shoppingCartId)
    {
        var context = _hca.HttpContext!;
        var transactionId = context.Request.Query["transactionId"];
        var response = await _exactlyService.GetTransactionDetailsAsync(transactionId, cancellationToken: context.RequestAborted);
        var data = response.Attributes;

        switch (data.Processing.ResultCode, data.Status)
        {
            case (_, ChargeResponse.ChargeResponseStatus.Processing):
                return PaymentController.RedirectToWait(controller);
            case (_, ChargeResponse.ChargeResponseStatus.Processed):
            case ("success", _):
                return await _paymentService.UpdateAndRedirectToFinishedOrderAsync(
                    controller, order, shoppingCartId, ProviderName, _ => [response.ToPayment(_moneyService)]);
            case (_, ChargeResponse.ChargeResponseStatus.ActionRequired)
                when data.Actions?.FirstOrDefault(action => action.Attributes.IsGet) is { } action:
                return PaymentController.RedirectToWait(controller, action.Attributes.Url.AbsoluteUri);
            case ({ } resultCode, _) when !string.IsNullOrEmpty(resultCode):
                var resultCodes = ChargeResponse.GetResultCodes(_chargeResponseStringLocalizer);
                var resultMessage = resultCodes.GetMaybe(resultCode) ?? resultCodes["transaction_failed"];
                return await FailAsync(controller, order, H["Your transaction has failed: {0}.", resultMessage]);
            case (_, ChargeResponse.ChargeResponseStatus.Failed):
            case (_, ChargeResponse.ChargeResponseStatus.ActionRequired):
                return await FailAsync(controller, order, H["Your transaction has failed."]);
            default:
                throw new ArgumentOutOfRangeException(response.Attributes.Status.ToString());
        }
    }

    private async Task<IActionResult> FailAsync(Controller controller, ContentItem order, LocalizedHtmlString message)
    {
        await _notifier.ErrorAsync(message);
        return PaymentController.RedirectToWait(controller, controller.Url.Content("~/checkout"));
    }
}
