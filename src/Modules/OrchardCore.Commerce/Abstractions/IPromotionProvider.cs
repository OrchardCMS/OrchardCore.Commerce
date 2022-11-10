using OrchardCore.Commerce.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Promotion providers apply promotions to shopping cart items.
/// </summary>
public interface IPromotionProvider : ISortableUpdaterProvider<IList<ShoppingCartItem>>
{ }
