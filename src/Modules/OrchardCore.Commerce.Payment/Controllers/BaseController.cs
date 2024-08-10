using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Core.Utilities;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Controllers;
public abstract class BaseController : Controller
{
    private readonly INotifier _notifier;
    protected BaseController(INotifier notifier) => _notifier = notifier;


    public async Task<IActionResult> ProduceResultAsync(PaidStatusViewModel paidStatusViewModel)
    {
        if (paidStatusViewModel.Status == PaidStatus.Suceeded)
        {
            if (paidStatusViewModel.ShowMessage != null)
            {
                await _notifier.SuccessAsync(paidStatusViewModel.ShowMessage);
            }

            return RedirectToActionWithParams<PaymentController>(
                nameof(PaymentController.Success),
                FeatureIds.Area,
                string.Empty,
                orderId: paidStatusViewModel.Content?.ContentItemId);
        }
        else if (paidStatusViewModel.Status == PaidStatus.Failed)
        {
            if (paidStatusViewModel.ShowMessage != null)
            {
                await _notifier.ErrorAsync(paidStatusViewModel.ShowMessage);
            }

            return RedirectToActionWithParams<PaymentController>(nameof(PaymentController.Index), FeatureIds.Payment);
        }
        else if (paidStatusViewModel.Status == PaidStatus.NotFound)
        {
            if (paidStatusViewModel.ShowMessage != null)
            {
                await _notifier.WarningAsync(paidStatusViewModel.ShowMessage);
            }

            return NotFound();
        }
        else if (paidStatusViewModel.Status == PaidStatus.NotThingToDo)
        {
            if (paidStatusViewModel.ShowMessage != null)
            {
                await _notifier.InformationAsync(paidStatusViewModel.ShowMessage);
            }

            return this.RedirectToContentDisplay(paidStatusViewModel.Content);
        }
        else if (paidStatusViewModel.Status == PaidStatus.WaitingStripe)
        {

            if (paidStatusViewModel.ShowMessage != null)
            {
                await _notifier.InformationAsync(paidStatusViewModel.ShowMessage);
            }

            return RedirectToActionWithNames("PaymentConfirmationMiddleware", "OrchardCore.Commerce.Payment.Stripe", "Stripe");
        }
        else if (paidStatusViewModel.Status == PaidStatus.WaitingPayment)
        {
            if (paidStatusViewModel.ShowMessage != null)
            {
                await _notifier.InformationAsync(paidStatusViewModel.ShowMessage);
            }

            return RedirectToActionWithParams<PaymentController>(nameof(PaymentController.Wait), FeatureIds.Payment, paidStatusViewModel.Url);
        }

        else
        {
            return NotFound();
        }
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
