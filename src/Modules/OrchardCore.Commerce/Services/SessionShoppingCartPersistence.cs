using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public sealed class SessionShoppingCartPersistence : ShoppingCartPersistenceBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;

    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public SessionShoppingCartPersistence(
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<IShoppingCartEvents> shoppingCartEvents,
        IShoppingCartSerializer shoppingCartSerializer)
        : base(shoppingCartEvents)
    {
        _httpContextAccessor = httpContextAccessor;
        _shoppingCartSerializer = shoppingCartSerializer;
    }

    protected override Task<ShoppingCart> RetrieveInnerAsync(string key)
    {
        var serialized = Session.GetString(key);
        if (serialized == null && _httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(key, out var serializedCart);
            return _shoppingCartSerializer.DeserializeAsync(serializedCart);
        }

        return _shoppingCartSerializer.DeserializeAsync(serialized);
    }

    protected override async Task<bool> StoreInnerAsync(string key, ShoppingCart items)
    {
        var cartString = await _shoppingCartSerializer.SerializeAsync(items);
        if (Session.GetString(key) == cartString) return false;

        Session.SetString(key, cartString);
        _httpContextAccessor.SetCookieForever(key, cartString);

        return true;
    }
}
