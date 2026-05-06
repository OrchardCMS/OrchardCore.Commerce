#nullable enable

using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class PaymentIntentPersistence : IPaymentIntentPersistence
{
    // Using _ as a separator to avoid separator character conflicts.
    private const string PaymentIntentKeyPrefix = "OrchardCore_Commerce_" + nameof(PaymentIntentPersistenceInfo);

    private readonly IHttpContextAccessor _httpContextAccessor;

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;
    private HttpRequest? Request => _httpContextAccessor.HttpContext?.Request;

    public PaymentIntentPersistence(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Task<PaymentIntentPersistenceInfo?> RetrieveAsync(string? shoppingCartId)
    {
        var key = GetCacheId(shoppingCartId);

        if (Session?.GetString(key)?.Trim() is { Length: > 0 } serializedFromSession &&
            TryParse(serializedFromSession, out var sessionResult))
        {
            return Task.FromResult(sessionResult);
        }

        if (Request != null &&
            Request.Cookies.TryGetValue(key, out var serializedFromCookie) &&
            TryParse(serializedFromCookie, out var cookieResult))
        {
            return Task.FromResult(cookieResult);
        }

        return Task.FromResult<PaymentIntentPersistenceInfo?>(null);
    }

    public Task StoreAsync(string? shoppingCartId, PaymentIntentPersistenceInfo info)
    {
        var key = GetCacheId(shoppingCartId);
        var serialized = JsonSerializer.Serialize(info);

        Session?.SetString(key, serialized);
        _httpContextAccessor.SetCookieForever(key, serialized);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string? shoppingCartId)
    {
        var key = GetCacheId(shoppingCartId);
        Session?.Remove(key);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(key);

        return Task.CompletedTask;
    }

    protected string GetCacheId(string? shoppingCartId) =>
       string.IsNullOrEmpty(shoppingCartId) ? PaymentIntentKeyPrefix : $"{PaymentIntentKeyPrefix}_{shoppingCartId}";

    private static bool TryParse(string serialized, out PaymentIntentPersistenceInfo? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(serialized)) return false;

        try
        {
            result = JsonSerializer.Deserialize<PaymentIntentPersistenceInfo?>(serialized);
            return !string.IsNullOrWhiteSpace(result?.PaymentIntentId);
        }
        catch
        {
            return false;
        }
    }
}
