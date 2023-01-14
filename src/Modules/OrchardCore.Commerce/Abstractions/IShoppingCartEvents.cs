using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Events related to <see cref="ShoppingCartController"/>.
/// </summary>
public interface IShoppingCartEvents
{
    /// <summary>
    /// Gets the value used to sort providers in ascending order.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Invoked after the shopping cart data is prepared, but before the shapes are rendered.
    /// </summary>
    /// <returns>The new versions of the matching parameters.</returns>
    Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        ShoppingCartDisplayingEventContext eventContext);

    /// <summary>
    /// Invoked before an item is added to the shopping cart to check whether it can be added based on inventory status.
    /// </summary>
    Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item);
}
