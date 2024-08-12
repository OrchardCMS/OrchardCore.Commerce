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

    public async Task<IActionResult> ProduceActionResultAsync(PaymentStatusViewModel paidStatusViewModel)
    {
        if (paidStatusViewModel.ShowMessage != null)
        {
            switch (paidStatusViewModel.Status)
            {
                case PaymentStatus.Succeeded:
                    await _notifier.SuccessAsync(paidStatusViewModel.ShowMessage);
                    break;
                case PaymentStatus.Failed:
                    await LogAndNotifyFailedAsync(paidStatusViewModel);
                    break;
                case PaymentStatus.NotFound:
                    await LogAndNotifyWarningAsync(paidStatusViewModel);
                    break;
                case PaymentStatus.NotThingToDo:
                case PaymentStatus.WaitingStripe:
                case PaymentStatus.WaitingPayment:
                    await LogAndNotifyInformationAsync(paidStatusViewModel);
                    break;
                default:
                    await LogAndNotifyFailedAsync(paidStatusViewModel);
                    break;
            }
        }

        return paidStatusViewModel.Status switch
        {
            PaymentStatus.Succeeded => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Success),
                FeatureIds.Area,
                orderId: paidStatusViewModel.Content?.ContentItemId),

            PaymentStatus.Failed => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Index),
                FeatureIds.Payment),

            PaymentStatus.NotFound => NotFound(),

            PaymentStatus.NotThingToDo => this.RedirectToContentDisplay(paidStatusViewModel.Content),

            PaymentStatus.WaitingStripe => RedirectToActionWithNames(
                "PaymentConfirmationMiddleware",
                "OrchardCore.Commerce.Payment.Stripe",
                "Stripe"),

            PaymentStatus.WaitingPayment => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Wait),
                FeatureIds.Payment,
                paidStatusViewModel.Url),

            _ => throw new ArgumentOutOfRangeException(paidStatusViewModel.ToString()),
        };
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

        object? routeValues = new { area, returnUrl = localReturnUrl };

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

    private RedirectToActionResult RedirectToActionWithNames(
        string actionName,
        string area,
        string controllerName,
        string? returnUrl = null,
        string? orderId = null)
    {
        string localReturnUrl = string.Empty;
        if (!string.IsNullOrEmpty(returnUrl))
        {
            localReturnUrl = string.IsNullOrEmpty(returnUrl)
                    ? HttpContext.Request.GetDisplayUrl()
                    : returnUrl;
        }

        object? routeValues = new { area, returnUrl = localReturnUrl };

        if (!string.IsNullOrEmpty(orderId))
        {
            routeValues = new { area, orderId, returnUrl = localReturnUrl };
        }

        return RedirectToAction(
            actionName,
            controllerName,
            routeValues
        );
    }

    private async Task LogAndNotifyFailedAsync(PaymentStatusViewModel paidStatusViewModel)
    {
        await _notifier.ErrorAsync(paidStatusViewModel.ShowMessage);
#pragma warning disable CA2254
        _logger.LogCritical(paidStatusViewModel.HideMessage);
#pragma warning restore CA2254
    }

    private async Task LogAndNotifyWarningAsync(PaymentStatusViewModel paidStatusViewModel)
    {
        await _notifier.WarningAsync(paidStatusViewModel.ShowMessage);
#pragma warning disable CA2254
        _logger.LogWarning(paidStatusViewModel.HideMessage);
#pragma warning restore CA2254
    }

    private async Task LogAndNotifyInformationAsync(PaymentStatusViewModel paidStatusViewModel)
    {
        await _notifier.InformationAsync(paidStatusViewModel.ShowMessage);
#pragma warning disable CA2254
        _logger.LogInformation(paidStatusViewModel.HideMessage);
#pragma warning restore CA2254
    }
}
