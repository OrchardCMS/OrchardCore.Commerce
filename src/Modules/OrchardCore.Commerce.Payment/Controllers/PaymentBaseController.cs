using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Controllers;
public abstract class PaymentBaseController : Controller
{
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    protected PaymentBaseController(INotifier notifier, ILogger logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    protected async Task<IActionResult> ProduceActionResultAsync(PaymentOperationStatusViewModel paidStatusViewModel)
    {
        if (paidStatusViewModel.ShowMessage != null)
        {
            switch (paidStatusViewModel.Status)
            {
                case PaymentOperationStatus.Succeeded:
                    await _notifier.SuccessAsync(paidStatusViewModel.ShowMessage);
                    break;
                case PaymentOperationStatus.Failed:
                    await LogAndNotifyFailedAsync(paidStatusViewModel);
                    break;
                case PaymentOperationStatus.NotFound:
                    await LogAndNotifyWarningAsync(paidStatusViewModel);
                    break;
                case PaymentOperationStatus.NotThingToDo:
                case PaymentOperationStatus.WaitingForRedirect:
                    await LogAndNotifyInformationAsync(paidStatusViewModel);
                    break;
                default:
                    await LogAndNotifyFailedAsync(paidStatusViewModel);
                    break;
            }
        }

#pragma warning disable SCS0027
        return paidStatusViewModel.Status switch
        {
            PaymentOperationStatus.Succeeded => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Success),
                FeatureIds.Area,
                orderId: paidStatusViewModel.Content?.ContentItemId),

            PaymentOperationStatus.Failed => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Index),
                FeatureIds.Payment),

            PaymentOperationStatus.NotFound => NotFound(),

            PaymentOperationStatus.NotThingToDo => paidStatusViewModel.Content is { } content
                ? this.RedirectToContentDisplay(content)
                : NotFound(),

            PaymentOperationStatus.WaitingForRedirect => Redirect(url: paidStatusViewModel.Url),

            _ => throw new ArgumentOutOfRangeException(paidStatusViewModel.ToString()),
        };
#pragma warning restore SCS0027
    }

    private RedirectToActionResult RedirectToActionWithParams<TController>(
        string actionName,
        string area,
        string? returnUrl = null,
        string? orderId = null)
        where TController : Controller
    {
        string localReturnUrl = string.Empty;
        if (!string.IsNullOrEmpty(returnUrl))
        {
            localReturnUrl = string.IsNullOrEmpty(returnUrl)
                    ? HttpContext.Request.GetDisplayUrl()
                    : returnUrl;
        }

        object routeValues = new { area, returnUrl = localReturnUrl };

        if (!string.IsNullOrEmpty(orderId))
        {
            routeValues = new { area, orderId, returnUrl = localReturnUrl };
        }

        return RedirectToAction(
            actionName,
            typeof(TController).ControllerName(),
            routeValues
        );
    }

    private async Task LogAndNotifyFailedAsync(PaymentOperationStatusViewModel paidStatusViewModel)
    {
        _logger.LogCritical("The payment provider encountered the following error: {Message}", paidStatusViewModel.HideMessage);
        await _notifier.ErrorAsync(paidStatusViewModel.ShowMessage);
    }

    private async Task LogAndNotifyWarningAsync(PaymentOperationStatusViewModel paidStatusViewModel)
    {
        _logger.LogWarning("The payment provider encountered the following warning: {Message}", paidStatusViewModel.HideMessage);
        await _notifier.WarningAsync(paidStatusViewModel.ShowMessage);
    }

    private async Task LogAndNotifyInformationAsync(PaymentOperationStatusViewModel paidStatusViewModel)
    {
        _logger.LogInformation("The payment provider encountered the following information: {Message}", paidStatusViewModel.HideMessage);
        await _notifier.InformationAsync(paidStatusViewModel.ShowMessage);
    }
}
