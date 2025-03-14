using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

/// <summary>
/// Extension points for events related to orders.
/// </summary>
public interface IOrderEvents
{
    /// <summary>
    /// Invoked when a new free (non-payment) order is created.
    /// </summary>
    Task CreatedFreeAsync(OrderPart orderPart, ShoppingCart cart, ShoppingCartViewModel viewModel) => Task.CompletedTask;

    /// <summary>
    /// Invoked when the <paramref name="order"/> is set to the <c>Ordered</c> state.
    /// </summary>
    Task OrderedAsync(ContentItem order, string shoppingCartId) => Task.CompletedTask;

    /// <summary>
    /// Invoked during cleanup after the order has been finalized.
    /// </summary>
    Task FinalizeAsync(ContentItem order, string shoppingCartId, string paymentProviderName) => Task.CompletedTask;
}
