using System;
using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    [Serializable]
    public sealed class ShoppingCartItem : IEquatable<ShoppingCartItem>
    {
        public int Quantity { get; }

        public ContentItem Product { get; }

        public IList<IPrice> Prices { get; } = new List<IPrice>();

        public ShoppingCartItem(int quantity, ContentItem product)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity = quantity;
            Product = product ?? throw new ArgumentNullException(nameof(Product));
        }

        public override string ToString()
        {
            return $"{Quantity} x {Product.DisplayText}";
        }

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
            && (ReferenceEquals(this, obj) || Equals(obj as ShoppingCartItem));

        public bool Equals(ShoppingCartItem other)
            => other == null ? false : other.Quantity == Quantity && other.Product.Equals(Product);

        public override int GetHashCode() => HashCode.Combine(Product, Quantity);
    }
}
