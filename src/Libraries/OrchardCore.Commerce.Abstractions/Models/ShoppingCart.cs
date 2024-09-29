using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Models;

/// <summary>
/// A shopping cart.
/// </summary>
public class ShoppingCart
{
    public string Id { get; set; }

    /// <summary>
    /// Gets the list of quantities of product variants in the cart.
    /// </summary>
    public IList<ShoppingCartItem> Items { get; }

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

    public ShoppingCart()
        : this(Enumerable.Empty<ShoppingCartItem>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
    /// </summary>
    /// <param name="items">The list of product variant quantities to copy onto the new cart.</param>
    /// <remarks>
    /// <para>
    /// The constructor always copies the contents of <paramref name="items"/> into a new list to avoid accidentally
    /// sharing it and mutating the object passed in.
    /// </para>
    /// </remarks>
    public ShoppingCart(IEnumerable<ShoppingCartItem> items) =>
        Items = new List<ShoppingCartItem>(items ?? Enumerable.Empty<ShoppingCartItem>());

    /// <summary>
    /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
    /// </summary>
    /// <param name="items">The list of product variant quantities to copy onto the new cart.</param>
    public ShoppingCart(params ShoppingCartItem[] items)
        : this((IList<ShoppingCartItem>)items)
    {
    }

    /// <summary>
    /// Clones the shopping cart, replacing its list of items with a copy of the one provided.
    /// </summary>
    /// <param name="items">The new list of items.</param>
    /// <returns>A new shopping cart with all properties identical to this, but with a different list of items.</returns>
    [SuppressMessage(
        "Performance",
        "CA1822",
        Justification = $"Keep non-static in case {nameof(ShoppingCart)} gets additional properties in the future.")]
    public ShoppingCart With(IEnumerable<ShoppingCartItem> items) => new(items);

    /// <summary>
    /// Adds a quantity of product variants into the cart. If the product variant already exists in the cart, the
    /// quantity gets updated. Otherwise, it's added to the end of the list.
    /// </summary>
    /// <param name="item">The cart item to add.</param>
    public ShoppingCartItem AddItem(ShoppingCartItem item)
    {
        var existingIndex = IndexOf(item);
        if (existingIndex != -1)
        {
            var existingItem = Items[existingIndex];
            item = item.WithQuantity(existingItem.Quantity + item.Quantity);
            Items.Remove(existingItem);
        }

        Items.Add(item);
        return item;
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    /// <param name="item">The product variant to remove. Quantity will be ignored.</param>
    public void RemoveItem(ShoppingCartItem item)
    {
        if (Items.FirstOrDefault(line => line.IsSameProductAs(item)) is { } itemToRemove)
        {
            Items.Remove(itemToRemove);
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

        Items[existingIndex] = item.WithPrices(prioritizedPrices);
    }

    /// <summary>
    /// Finds the index of the first line in the cart with the same product variant as the passed in item (quantity may
    /// be different).
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    private int IndexOf(ShoppingCartItem item)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].IsSameProductAs(item))
            {
                return i;
            }
        }

        return -1;
    }
}
