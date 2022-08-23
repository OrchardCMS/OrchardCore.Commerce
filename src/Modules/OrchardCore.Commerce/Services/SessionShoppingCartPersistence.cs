using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class SessionShoppingCartPersistence : IShoppingCartPersistence
{
    private const string ShoppingCartPrefix = "OrchardCore:Commerce:ShoppingCart:";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShoppingCartSerializer _shoppingCartSerializer;

    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public SessionShoppingCartPersistence(
        IHttpContextAccessor httpContextAccessor,
        IShoppingCartSerializer shoppingCartSerializer)
    {
        _httpContextAccessor = httpContextAccessor;
        _shoppingCartSerializer = shoppingCartSerializer;
    }

    public string GetUniqueCartId(string shoppingCartId) => Session.Id + shoppingCartId;

    public Task<ShoppingCart> RetrieveAsync(string shoppingCartId = null)
    {
        var cartString = Session.GetString(ShoppingCartPrefix + (shoppingCartId ?? string.Empty));
        return _shoppingCartSerializer.DeserializeAsync(cartString);
    }

    public async Task StoreAsync(ShoppingCart items, string shoppingCartId = null)
    {
        var cartString = await _shoppingCartSerializer.SerializeAsync(items);
        Session.SetString(ShoppingCartPrefix + (shoppingCartId ?? string.Empty), cartString);
    }
}
