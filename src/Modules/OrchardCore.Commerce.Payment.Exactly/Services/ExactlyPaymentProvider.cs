using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Controllers;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Settings;
using System;
using System.Threading.Tasks;

using PaymentFeatureIds = OrchardCore.Commerce.Payment.Constants.FeatureIds;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Exactly";

    private readonly IContentManager _contentManager;
    private readonly IExactlyService _exactlyService;
    private readonly IHttpContextAccessor _hca;
    private readonly INotifier _notifier;
    private readonly IPaymentService _paymentService;
    private readonly ISiteService _siteService;
    private readonly IHtmlLocalizer<ExactlyPaymentProvider> H;

    public string Name => ProviderName;

    public ExactlyPaymentProvider(
        IContentManager contentManager,
        IExactlyService exactlyService,
        IHttpContextAccessor hca,
        INotifier notifier,
        IPaymentService paymentService,
        ISiteService siteService,
        IHtmlLocalizer<ExactlyPaymentProvider> htmlLocalizer)
    {
        _contentManager = contentManager;
        _exactlyService = exactlyService;
        _hca = hca;
        _notifier = notifier;
        _paymentService = paymentService;
        _siteService = siteService;
        H = htmlLocalizer;
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

        switch (response.Attributes.Status)
        {
            case ChargeResponse.ChargeResponseStatus.ActionRequired:
            case ChargeResponse.ChargeResponseStatus.Processing:
                return controller.RedirectToAction(
                    nameof(PaymentController.Wait),
                    typeof(PaymentController).ControllerName(),
                    new { area = PaymentFeatureIds.Payment, returnUrl = context.Request.GetDisplayUrl() });
            case ChargeResponse.ChargeResponseStatus.Processed:
                return await _paymentService.UpdateAndRedirectToFinishedOrderAsync(
                    controller, order, shoppingCartId, ProviderName, _ => [response.ToPayment()]);
            case ChargeResponse.ChargeResponseStatus.Failed:
                order.Alter<OrderPart>(part => part.Status.Text = OrderStatuses.PaymentFailed.HtmlClassify());
                await _contentManager.PublishAsync(order);

                await _notifier.ErrorAsync(H["Your transaction has failed."]);
                return controller.Redirect("~/cart");
            default:
                throw new ArgumentOutOfRangeException(response.Attributes.Status.ToString());
        }
    }
}
