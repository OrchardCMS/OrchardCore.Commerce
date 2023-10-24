using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Services related to payment and <see cref="PaymentController"/>.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates and returns <see cref="CheckoutViewModel"/>.
    /// </summary>
    Task<CheckoutViewModel> CreateCheckoutViewModelAsync(
        string shoppingCartId,
        Action<OrderPart> updateOrderPart = null);

    /// <summary>
    /// When the order is payed this logic should be run to set <paramref name="order"/> properties that represents its state.
    /// </summary>
    Task FinalModificationOfOrderAsync(ContentItem order);

    /// <summary>
    /// Creates an order content item without payment in the database based on the current <see cref="ShoppingCart"/> content.
    /// </summary>
    Task<ContentItem> CreateNoPaymentOrderFromShoppingCartAsync();

    /// <summary>
    /// Updates the <paramref name="order"/>'s status to <see cref="OrderStatuses.Ordered"/>.
    /// </summary>
    Task UpdateOrderToOrderedAsync(ContentItem order, Action<OrderPart> alterOrderPart = null);
}
