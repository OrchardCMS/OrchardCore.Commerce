using System.Collections.Generic;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// Price providers add prices to shopping cart items.
    /// 
    /// </summary>
    public interface IPriceProvider
    {
        /// <summary>
        /// Adds prices to shopping cart items.
        /// </summary>
        /// <param name="items">The quantities and products to which prices must be added.</param>
        void AddPrices(IList<ShoppingCartItem> items);
    }
}
