using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public abstract class ShoppingCartPersistenceBase : IShoppingCartPersistence
{
    private const string ShoppingCartPrefix = "OrchardCore:Commerce:ShoppingCart";

    private readonly Dictionary<string, JObject> _scopeCache = new();

    protected readonly IEnumerable<IShoppingCartEvents> _shoppingCartEvents;

    protected ShoppingCartPersistenceBase(IEnumerable<IShoppingCartEvents> shoppingCartEvents) =>
        _shoppingCartEvents = shoppingCartEvents;

    public async Task<ShoppingCart> RetrieveAsync(string shoppingCartId)
    {
        var key = GetCacheId(shoppingCartId);

        // JSON object is stored for the sake of deep cloning. This ensures any modification of the returned instance
        // won't propagate back the next time RetrieveAsync is called.
        if (_scopeCache.TryGetValue(key, out var jObject)) return jObject.ToObject<ShoppingCart>();

        var cart = await RetrieveInnerAsync(key) ?? new ShoppingCart();
        cart.Id = shoppingCartId;

        foreach (var shoppingCartEvent in _shoppingCartEvents.OrderBy(provider => provider.Order))
        {
            cart = await shoppingCartEvent.LoadedAsync(cart) ?? cart;
        }

        _scopeCache[key] = JObject.FromObject(cart, new JsonSerializer());
        return cart;
    }

    public async Task StoreAsync(ShoppingCart items)
    {
        var key = GetCacheId(items.Id);

        if (await StoreInnerAsync(key, items))
        {
            _scopeCache.Remove(key, out _);
        }
    }

    /// <summary>
    /// Retrieves the items using the <see cref="ShoppingCartPersistenceBase"/>-specific <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A prefix and (if set) the shopping cart ID combined.</param>
    /// <returns>The retrieved <see cref="ShoppingCart"/> instance or <see langword="null"/> if none found.</returns>
    protected abstract Task<ShoppingCart> RetrieveInnerAsync(string key);

    /// <summary>
    /// Stores the provided <paramref name="items"/> using the <see cref="ShoppingCartPersistenceBase"/>-specific
    /// <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A prefix and (if set) the shopping cart ID combined.</param>
    /// <param name="items">The shopping cart to be stored.</param>
    /// <returns>
    /// A value indicating whether the cache should be invalidated. This prevents issues if a new value is stored and
    /// then retrieved.
    /// </returns>
    protected abstract Task<bool> StoreInnerAsync(string key, ShoppingCart items);

    protected string GetCacheId(string shoppingCartId) =>
        string.IsNullOrEmpty(shoppingCartId) ? ShoppingCartPrefix : $"{ShoppingCartPrefix}:{shoppingCartId}";
}
