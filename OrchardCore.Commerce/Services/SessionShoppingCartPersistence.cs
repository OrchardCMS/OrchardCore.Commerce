using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class SessionShoppingCartPersistence : IShoppingCartPersistence
{
    private const string ShoppingCartPrefix = "OrchardCore:Commerce:ShoppingCart:";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;

    public SessionShoppingCartPersistence(
        IHttpContextAccessor httpContextAccessor,
        IShoppingCartHelpers shoppingCartHelpers)
    {
        _httpContextAccessor = httpContextAccessor;
        _shoppingCartHelpers = shoppingCartHelpers;
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public string GetUniqueCartId(string shoppingCartId) =>
        Session.Id + shoppingCartId;

    public Task<ShoppingCart> RetrieveAsync(string shoppingCartId = null)
    {
        var cartString = Session.GetString(ShoppingCartPrefix + (shoppingCartId ?? string.Empty));
        return _shoppingCartHelpers.DeserializeAsync(cartString);
    }

    public async Task StoreAsync(ShoppingCart items, string shoppingCartId = null)
    {
        var cartString = await _shoppingCartHelpers.SerializeAsync(items);
        Session.SetString(ShoppingCartPrefix + (shoppingCartId ?? string.Empty), cartString);
    }
}
