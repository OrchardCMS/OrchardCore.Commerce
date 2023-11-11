using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

/// <summary>
/// Extension points for events related to checkout.
/// </summary>
public interface ICheckoutEvents
{
    /// <summary>
    /// Invoked at the start of a new <see cref="OrderPart"/> creation which is used to create the <see
    /// cref="ICheckoutViewModel"/>.
    /// </summary>
    Task OrderCreatingAsync(OrderPart orderPart, string shoppingCartId) => Task.CompletedTask;

    /// <summary>
    /// Invoked after the creation of <see cref="ICheckoutViewModel"/> to check whether the model is invalid.
    /// </summary>
    Task<bool> ViewModelCreatedAsync(IList<ShoppingCartLineViewModel> lines) => Task.FromResult(false);
}
