using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A shopping cart.
/// </summary>
public class ShoppingCart
{
    private readonly List<ShoppingCartItem> _items;

    /// <summary>
    /// Gets the list of quantities of product variants in the cart.
    /// </summary>
    public IList<ShoppingCartItem> Items => _items;

    public ShoppingCart()
        : this(items: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
    /// </summary>
    /// <param name="items">The list of product variant quantities to copy onto the new cart.</param>
    public ShoppingCart(IEnumerable<ShoppingCartItem> items)
    {
        items ??= Enumerable.Empty<ShoppingCartItem>();
        _items = items as List<ShoppingCartItem> ?? new List<ShoppingCartItem>(items);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
    /// </summary>
    /// <param name="items">The list of product variant quantities to copy onto the new cart.</param>
    public ShoppingCart(params ShoppingCartItem[] items)
        : this((IList<ShoppingCartItem>)items)
    {
    }

    /// <summary>
    /// Gets the number of lines in the cart.
    /// </summary>
    [JsonIgnore]
    public int Count => Items.Count;

    /// <summary>
    /// Gets the total number of items (i.e. products) in the cart. In other words, the sum of quantities of all lines.
    /// </summary>
    [JsonIgnore]
    public int ItemCount => Items.Sum(item => item.Quantity);

    /// <summary>
    /// Clones the shopping cart, replacing its list of items with a copy of the one provided.
    /// </summary>
    /// <param name="items">The new list of items.</param>
    /// <returns>A new shopping cart with all properties identical to this, but with a different list of items.</returns>
    [SuppressMessage(
        "Performance",
        "CA1822",
        Justification = $"Reserved in case {nameof(ShoppingCart)} gets additional properties in the future.")]
    public ShoppingCart With(IEnumerable<ShoppingCartItem> items) => new(items);

    /// <summary>
    /// Adds a quantity of product variants into the cart. If the product variant already exists in the cart, the
    /// quantity gets updated. Otherwise, it's added to the end of the list.
    /// </summary>
    /// <param name="item">The cart item to add.</param>
    public void AddItem(ShoppingCartItem item)
    {
        var existingIndex = IndexOf(item);
        if (existingIndex != -1)
        {
            var existingItem = Items[existingIndex];
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
        var existingIndex = IndexOf(item);
        if (existingIndex != -1)
        {
            Items.Remove(Items[existingIndex]);
        }
    }

    /// <summary>
    /// Sets the list of prices for the item.
    /// </summary>
    public void SetPrices(ShoppingCartItem item, IEnumerable<PrioritizedPrice> prioritizedPrices)
    {
        var existingIndex = IndexOf(item);
        if (existingIndex == -1)
        {
            throw new InvalidOperationException(
                $"Can't set prices for product \"{item.ProductSku}\" because it's not in the cart.");
        }

        Items.Remove(Items[existingIndex]);
        Items.Insert(existingIndex, item.WithPrices(prioritizedPrices));
    }

    /// <summary>
    /// Finds the index of the first line in the cart with the same product variant as the passed in item (quantity may
    /// be different).
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    private int IndexOf(ShoppingCartItem item) => _items.FindIndex(line => line.IsSameProductAs(item));
}
