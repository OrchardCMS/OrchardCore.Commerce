using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Helpers
{
    public static class ShoppingCartHelpers
    {
        public static ShoppingCartItem GetExistingItem(this IList<ShoppingCartItem> cart, ShoppingCartItem item)
            => cart.FirstOrDefault(i => i.IsSameProductAs(item));

        public static int RemoveItem(this IList<ShoppingCartItem> cart, ShoppingCartItem item)
        {
            var index = cart.IndexOfProduct(item);
            if (index != -1)
            {
                cart.RemoveAt(index);
            }
            return index;
        }

        public static int IndexOfProduct(this IList<ShoppingCartItem> cart, ShoppingCartItem item)
        {
            var index = 0;
            foreach(var i in cart)
            {
                if (i.IsSameProductAs(item)) return index;
                index++;
            }
            return -1;
        }

        public static bool IsSameProductAs(this ShoppingCartItem item, ShoppingCartItem other)
            => other.ProductSku == item.ProductSku
                && ((item.Attributes is null && other.Attributes is null)
                || (item.Attributes != null && other.Attributes != null && other.Attributes.SetEquals(item.Attributes)));
    }
}
