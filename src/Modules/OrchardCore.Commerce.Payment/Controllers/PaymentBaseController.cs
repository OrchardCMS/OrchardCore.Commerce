using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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
    protected PaymentBaseController(INotifier notifier) => _notifier = notifier;

    public async Task<IActionResult> ProduceResultAsync(PaidStatusViewModel paidStatusViewModel)
    {
        if (paidStatusViewModel.ShowMessage != null)
        {
            switch (paidStatusViewModel.Status)
            {
                case PaidStatus.Succeeded:
                    await _notifier.SuccessAsync(paidStatusViewModel.ShowMessage);
                    break;
                case PaidStatus.Failed:
                    await _notifier.ErrorAsync(paidStatusViewModel.ShowMessage);
                    break;
                case PaidStatus.NotFound:
                    await _notifier.WarningAsync(paidStatusViewModel.ShowMessage);
                    break;
                case PaidStatus.NotThingToDo:
                case PaidStatus.WaitingStripe:
                case PaidStatus.WaitingPayment:
                    await _notifier.InformationAsync(paidStatusViewModel.ShowMessage);
                    break;
                default:
                    await _notifier.ErrorAsync(paidStatusViewModel.ShowMessage);
                    break;
            }
        }

        return paidStatusViewModel.Status switch
        {
            PaidStatus.Succeeded => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Success),
                FeatureIds.Area,
                string.Empty,
                orderId: paidStatusViewModel.Content?.ContentItemId),

            PaidStatus.Failed => RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Index),
                FeatureIds.Payment),

            PaidStatus.NotFound => NotFound(),

            PaidStatus.NotThingToDo => this.RedirectToContentDisplay(paidStatusViewModel.Content),

            PaidStatus.WaitingStripe => RedirectToActionWithNames(
                "PaymentConfirmationMiddleware",
                "OrchardCore.Commerce.Payment.Stripe",
                "Stripe"),

            PaidStatus.WaitingPayment => RedirectToActionWithParams<PaymentController>(
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
}
