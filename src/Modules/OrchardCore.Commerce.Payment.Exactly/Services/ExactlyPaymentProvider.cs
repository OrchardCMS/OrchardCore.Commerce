using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
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
    private readonly IContentManager _contentManager;
    private readonly IExactlyService _exactlyService;
    private readonly IHttpContextAccessor _hca;
    private readonly IMoneyService _moneyService;
    private readonly IPaymentService _paymentService;
    private readonly ISiteService _siteService;
    private readonly IHtmlLocalizer<ExactlyPaymentProvider> H;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public string Name => ProviderName;

#pragma warning disable S107 // Methods should not have too many parameters
    public ExactlyPaymentProvider(
        IStringLocalizer<ChargeResponse> chargeResponseStringLocalizer,
        IExactlyService exactlyService,
        IMoneyService moneyService,
        INotifier notifier,
        IPaymentService paymentService,
        IOrchardServices<ExactlyPaymentProvider> services,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        _chargeResponseStringLocalizer = chargeResponseStringLocalizer;
        _contentManager = services.ContentManager.Value;
        _exactlyService = exactlyService;
        _hca = services.HttpContextAccessor.Value;
        _moneyService = moneyService;
        _paymentService = paymentService;
        _siteService = services.SiteService.Value;
        H = services.HtmlLocalizer.Value;
        _actionContextAccessor = actionContextAccessor;
        _urlHelperFactory = urlHelperFactory;
    }

    public async Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model, bool isPaymentRequest = false, string shoppingCartId = null)
    {
        var settings = (await _siteService.GetSiteSettingsAsync())?.As<ExactlySettings>();
        return string.IsNullOrEmpty(settings?.ApiKey) || string.IsNullOrEmpty(settings.ProjectId) ? null : new object();
    }

    public async Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
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
                return new PaymentOperationStatusViewModel
                {
                    Status = PaymentOperationStatus.WaitingForRedirect,
                    Url = await BuildAbsoluteUrlAsync(nameof(PaymentController.Wait), typeof(PaymentController).ControllerName()),
                };
            case (_, ChargeResponse.ChargeResponseStatus.Processed):
            case ("success", _):
                return await SuccessAsync(order, shoppingCartId, response, H);
            case (_, ChargeResponse.ChargeResponseStatus.ActionRequired)
                when data.Actions?.FirstOrDefault(action => action.Attributes.IsGet) is { } action:
                return new PaymentOperationStatusViewModel
                {
                    Status = PaymentOperationStatus.WaitingForRedirect,
                    Url = action.Attributes.Url.AbsoluteUri,
                };
            case ({ } resultCode, _) when !string.IsNullOrEmpty(resultCode):
                var resultCodes = ChargeResponse.GetResultCodes(_chargeResponseStringLocalizer);
                var resultMessage = resultCodes.GetMaybe(resultCode) ?? resultCodes["transaction_failed"];
                return await FailAsync(order, H["Your transaction has failed: {0}.", resultMessage]);
            case (_, ChargeResponse.ChargeResponseStatus.Failed):
            case (_, ChargeResponse.ChargeResponseStatus.ActionRequired):
                return await FailAsync(order, H["Your transaction has failed."]);
            default:
                throw new ArgumentOutOfRangeException(response.Attributes.Status.ToString());
        }
    }

    private async Task<PaymentOperationStatusViewModel> FailAsync(ContentItem order, LocalizedHtmlString message)
    {
        order.Alter<OrderPart>(part => part.FailPayment());
        await _contentManager.UpdateAsync(order);

        return new PaymentOperationStatusViewModel
        {
            Status = PaymentOperationStatus.Failed,
            ShowMessage = message,
        };
    }

    private async Task<PaymentOperationStatusViewModel> SuccessAsync(
        ContentItem order,
        string shoppingCartId,
        ChargeResponse response,
        IHtmlLocalizer<ExactlyPaymentProvider> htmlLocalizer)
    {
        try
        {
            return await _paymentService.UpdateAndRedirectToFinishedOrderAsync(
                    order, shoppingCartId, ProviderName, _ => [response.ToPayment(_moneyService)]);
        }
        catch (Exception ex)
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.Failed,
                ShowMessage = htmlLocalizer["You have paid the bill, but this system did not record it. Please contact the administrators."],
                HideMessage = ex.Message,
                Content = order,
            };
        }
    }

    private async Task<string> BuildAbsoluteUrlAsync(
        string actionName,
        string controllerName,
        string areaName = FeatureIds.Payment,
        string returnUrl = null)
    {
        string url;
        var actionContext = _actionContextAccessor.ActionContext;
        actionContext ??= await GetActionContextAsync(_hca.HttpContext);
        var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
        url = urlHelper.ToAbsoluteAction(actionName, controllerName, new
        {
            area = areaName,
            returnUrl = string.IsNullOrEmpty(returnUrl)
                    ? _hca.HttpContext.Request.GetDisplayUrl()
                    : returnUrl,
        });
        return url;
    }

    internal static async Task<ActionContext> GetActionContextAsync(HttpContext httpContext)
    {
        var routeData = new RouteData();
        routeData.Routers.Add(new RouteCollection());

        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        var filters = httpContext.RequestServices.GetServices<IAsyncViewActionFilter>();

        foreach (var filter in filters)
        {
            await filter.OnActionExecutionAsync(actionContext);
        }

        return actionContext;
    }
}
