using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A shopping cart.
    /// </summary>
    public class ShoppingCart
    {
        /// <summary>
        /// The list of quantities of product variants in the cart.
        /// </summary>
        public IList<ShoppingCartItem> Items { get; }

        /// <summary>
        /// Constructs an empty cart.
        /// </summary>
        public ShoppingCart() : this(null) { }

        /// <summary>
        /// Constructs a cart from a list of product variant quantities.
        /// </summary>
        /// <param name="items">The list of product variant quantities to copy onto the new cart.</param>
        public ShoppingCart(IEnumerable<ShoppingCartItem> items)
        {
            Items = items is null ? new List<ShoppingCartItem>() : new List<ShoppingCartItem>(items);
        }

        /// <summary>
        /// Constructs a cart from a list of product variant quantities.
        /// </summary>
        /// <param name="items">The list of product variant quantities to copy onto the new cart.</param>
        public ShoppingCart(params ShoppingCartItem[] items) : this((IList<ShoppingCartItem>)items) { }

        /// <summary>
        /// The number of lines in the cart.
        /// </summary>
        [JsonIgnore]
        public int Count => Items.Count;

        /// <summary>
        /// The total number of items (i.e. products) in the cart.
        /// In other words, the sum of quantities of all lines.
        /// </summary>
        [JsonIgnore]
        public int ItemCount => Items.Sum(item => item.Quantity);

        /// <summary>
        /// Clones the shopping cart, replacing its list of items with a copy of the one provided.
        /// </summary>
        /// <param name="items">The new list of items.</param>
        /// <returns>A new shopping cart with all properties identical to this, but with a different list of items.</returns>
        public ShoppingCart With(IEnumerable<ShoppingCartItem> items) => new ShoppingCart(items);

        /// <summary>
        /// Adds a quantity of product variants into the cart.
        /// If the product variant already exists in the cart, the quantity gets updated. Otherwise, it's added to the end of the list.
        /// </summary>
        /// <param name="item">The cart item to add.</param>
        public void AddItem(ShoppingCartItem item)
        {
            int existingIndex = IndexOf(item);
            if (existingIndex != -1)
            {
                ShoppingCartItem existingItem = Items[existingIndex];
                Items[existingIndex] = existingItem.WithQuantity(existingItem.Quantity + item.Quantity);
            }
            else
            {
                Items.Add(item);
            }
        }

        /// <summary>
        /// Removes an item from the cart.
        /// </summary>
        /// <param name="item">The product variant to remove. Quantity will be ignored.</param>
        public void RemoveItem(ShoppingCartItem item)
        {
            int existingIndex = IndexOf(item);
            if (existingIndex != -1)
            {
                Items.Remove(Items[existingIndex]);
            }
        }

        /// <summary>
        /// Sets the list of prices for the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="prices">The list of prices.</param>
        public void SetPrices(ShoppingCartItem item, IEnumerable<PrioritizedPrice> prices)
        {
            int existingIndex = IndexOf(item);
            if (existingIndex != -1)
            {
                Items.Remove(Items[existingIndex]);
                Items.Insert(existingIndex, item.WithPrices(prices));
            }
            else throw new InvalidOperationException("Can't set prices on a product that's not in the cart.");
        }

        /// <summary>
        /// Finds the index of the first line in the cart with the same product variant as the passed in item (quantity may be different).
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <returns>The index of the item, or -1 if not found.</returns>
        private int IndexOf(ShoppingCartItem item)
        {
            var index = 0;
            foreach (ShoppingCartItem line in Items)
            {
                if (line.IsSameProductAs(item)) return index;
                index++;
            }
            return -1;
        }

    }
}
