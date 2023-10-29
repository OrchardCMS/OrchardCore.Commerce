using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Extension points for events related to orders.
/// </summary>
public interface IOrderEvents
{
    /// <summary>
    /// Invoked when a new free (non-payment) order is created.
    /// </summary>
    /// <param name="orderPart"></param>
    /// <param name="cart"></param>
    /// <param name="viewModel"></param>
    /// <param name="shoppingCart"></param>
    Task CreatedFreeAsnyc(OrderPart orderPart, ShoppingCart cart, ShoppingCartViewModel viewModel) => Task.CompletedTask;

    /// <summary>
    /// Invoked when the <paramref name="order"/> is set to the <c>Ordered</c> state.
    /// </summary>
    Task OrderedAsync(ContentItem order, string shoppingCartId) => Task.CompletedTask;

    /// <summary>
    /// Invoked during cleanup after the order has been finalized.
    /// </summary>
    Task FinalizeAsync(ContentItem order, string shoppingCartId, string paymentProviderName) => Task.CompletedTask;
}
