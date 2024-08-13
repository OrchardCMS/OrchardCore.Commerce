using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.ViewModels;

public static class UpdateModelExtensions
{
    private const string ShippingPrefix = $"{nameof(OrderPart)}.{nameof(OrderPart.ShippingAddress)}";
    private const string BillingPrefix = $"{nameof(OrderPart)}.{nameof(OrderPart.BillingAddress)}";

    public static async Task<(AddressFieldViewModel Shipping, AddressFieldViewModel Billing)> CreateOrderPartAddressViewModelsAsync(
        this IUpdateModel updater)
    {
        var shippingViewModel = new AddressFieldViewModel();
        var billingViewModel = new AddressFieldViewModel();
        if (!await updater.TryUpdateModelAsync(shippingViewModel, ShippingPrefix) ||
            !await updater.TryUpdateModelAsync(billingViewModel, BillingPrefix))
        {
            throw new InvalidOperationException(updater.GetModelErrorMessages().JoinNotNullOrEmpty());
        }

        return (Shipping: shippingViewModel, Billing: billingViewModel);
    }
}
