using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Services related to payment and <c>PaymentController</c>.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates and returns <see cref="ICheckoutViewModel"/>.
    /// </summary>
    Task<ICheckoutViewModel> CreateCheckoutViewModelAsync(
        string shoppingCartId,
        Action<OrderPart> updateOrderPart = null);

    /// <summary>
    /// When the order is payed this logic should be run to set <paramref name="order"/> properties that represents its state.
    /// </summary>
    Task FinalModificationOfOrderAsync(ContentItem order, string shoppingCartId);

    /// <summary>
    /// Creates an order content item without payment in the database based on the current <see cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateNoPaymentOrderFromShoppingCartAsync(string shoppingCartId);

    /// <summary>
    /// Updates the <paramref name="order"/>'s status to <see cref="OrderStatuses.Ordered"/>.
    /// </summary>
    /// <param name="getCharges">
    /// A callback to set the charges of the order. If returns <see langword="null"/> then nothing the <see
    /// cref="OrderPart.Charges"/> won't be altered, otherwise it will be replaced with the returned value.
    /// </param>
    Task UpdateOrderToOrderedAsync(
        ContentItem order,
        string shoppingCartId,
        Func<OrderPart, IEnumerable<IPayment>> getCharges = null);
}
