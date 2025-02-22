using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Exceptions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Abstractions;

/// <summary>
/// Services related to payment and <c>PaymentController</c>.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates and returns <see cref="ICheckoutViewModel"/>.
    /// </summary>
    Task<ICheckoutViewModel?> CreateCheckoutViewModelAsync(
        string? shoppingCartId,
        Action<OrderPart>? updateOrderPart = null);

    /// <summary>
    /// Calculates the shopping cart's checkout total.
    /// </summary>
    public Task<Amount> GetTotalAsync(string? shoppingCartId);

    /// <summary>
    /// When the order is paid this logic should be run to set <paramref name="order"/> properties that represents its state.
    /// </summary>
    Task FinalModificationOfOrderAsync(ContentItem order, string? shoppingCartId, string? paymentProviderName);

    /// <summary>
    /// Creates an order content item without payment in the database based on the current <see cref="ShoppingCart"/> content.
    /// </summary>
    /// <param name="mustBeFree">
    /// If <see langword="true"/>, then the order totals will be checked. They must be zero, otherwise an error
    /// notification will be sent and <see langword="null"/> is returned. If <see langword="false"/>, this is ignored.
    /// </param>
    Task<ContentItem?> CreatePendingOrderFromShoppingCartAsync(
        string? shoppingCartId,
        bool mustBeFree = false,
        bool notifyOnError = true,
        bool throwOnError = false);

    /// <summary>
    /// Updates the <paramref name="order"/>'s status to <see cref="OrderStatuses.Ordered"/>.
    /// </summary>
    /// <param name="getCharges">
    /// A callback to set the charges of the order. If returns <see langword="null"/> then nothing the <see
    /// cref="OrderPart.Charges"/> won't be altered, otherwise it will be replaced with the returned value.
    /// </param>
    Task UpdateOrderToOrderedAsync(
        ContentItem order,
        string? shoppingCartId,
        Func<OrderPart, IEnumerable<Commerce.Abstractions.Models.Payment>?>? getCharges = null);

    /// <summary>
    /// Tries to get the order identified by <paramref name="orderId"/>, or creates a new one if it's not there. Then
    /// updates the order's line items and address using the data in the <paramref name="updateModelAccessor"/> and the
    /// shopping cart identified by the <paramref name="shoppingCartId"/>. Additional alterations may be done via
    /// <paramref name="alterOrderAsync"/>, and finally the order is created or updated.
    /// </summary>
    /// <exception cref="FrontendException">Thrown if the order validation failed.</exception>
    Task<(ContentItem Order, bool IsNew)> CreateOrUpdateOrderFromShoppingCartAsync(
        IUpdateModelAccessor? updateModelAccessor,
        string? orderId,
        string? shoppingCartId,
        AlterOrderAsyncDelegate? alterOrderAsync = null,
        OrderPart? orderPart = null);

    /// <summary>
    /// Updates the provided Order content item from the update model as if it was just edited.
    /// </summary>
    Task<IList<string>> UpdateOrderWithDriversAsync(ContentItem order);

    /// <summary>
    /// Call back for payment.
    /// </summary>
    Task<PaymentOperationStatusViewModel> CallBackAsync(string paymentProviderName, string? orderId, string? shoppingCartId);

    /// <summary>
    /// Free checkout.
    /// </summary>
    Task<PaymentOperationStatusViewModel> CheckoutWithoutPaymentAsync(string? shoppingCartId, bool mustBeFree = true);

    /// <summary>
    /// Validate the checkout.
    /// </summary>
    /// <param name="providerName">The name of provider.</param>
    /// <param name="shoppingCartId">Shopping Cart.</param>
    /// <param name="paymentId">Payment Id, for Stripe,it is Payment Intent Id.</param>
    /// <returns>The list of the errors.</returns>
    Task<IList<string>> ValidateErrorsAsync(string providerName, string? shoppingCartId, string? paymentId);
}

public delegate Task AlterOrderAsyncDelegate(
    ContentItem order,
    bool isNew,
    Amount total,
    ShoppingCartViewModel? cartViewModel,
    IList<OrderLineItem> lineItems);

public static class PaymentServiceExtensions
{
    public static async Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
        this IPaymentService service,
        ContentItem order,
        string? shoppingCartId,
        string? paymentProviderName = null,
        Func<OrderPart, IEnumerable<Commerce.Abstractions.Models.Payment>?>? getCharges = null)
    {
        await service.UpdateOrderToOrderedAsync(order, shoppingCartId, getCharges);
        await service.FinalModificationOfOrderAsync(order, shoppingCartId, paymentProviderName);
        return new PaymentOperationStatusViewModel
        {
            Status = PaymentOperationStatus.Succeeded,
            Content = order,
        };
    }
}
