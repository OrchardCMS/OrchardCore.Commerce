using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Price providers add prices to shopping cart items.
/// </summary>
public interface IPriceProvider : ISortableUpdaterProvider<IList<ShoppingCartItem>>
{ }
