using System.Collections.Generic;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceService
    {
        /// <summary>
        /// Adds prices harvested from all price providers to shopping cart items, in order.
        /// </summary>
        /// <param name="items">The quantities and products to which prices must be added.</param>
        void AddPrices(IList<ShoppingCartItem> items);
    }
}
