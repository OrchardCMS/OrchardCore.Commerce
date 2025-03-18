using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class PaymentIntentPersistence : IPaymentIntentPersistence
{
    // Using _ as a separator to avoid separator character conflicts.
    private const string PaymentIntentKeyPrefix = "OrchardCore_Commerce_PaymentIntent";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public PaymentIntentPersistence(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Task<string> RetrieveAsync(string shoppingCartId = null)
    {
        var key = GetCacheId(shoppingCartId);
        var serialized = Session.GetString(key);
        if (serialized == null && _httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(key, out var serializedCart);
            return Task.FromResult(serializedCart);
        }

        return Task.FromResult(serialized);
    }

    public Task StoreAsync(string paymentIntentId, string shoppingCartId = null)
    {
        var key = GetCacheId(shoppingCartId);
        if (Session.GetString(key) == paymentIntentId) return Task.CompletedTask;

        Session.SetString(key, paymentIntentId);
        _httpContextAccessor.SetCookieForever(key, paymentIntentId);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string shoppingCartId = null)
    {
        var key = GetCacheId(shoppingCartId);
        Session.Remove(key);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(key);

        return Task.CompletedTask;
    }

    protected string GetCacheId(string shoppingCartId) =>
       string.IsNullOrEmpty(shoppingCartId) ? PaymentIntentKeyPrefix : $"{PaymentIntentKeyPrefix}_{shoppingCartId}";
}
