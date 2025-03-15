using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class PaymentIntentPersistence : IPaymentIntentPersistence
{
    // Using _ as a separator to avoid separator character conflicts.
    private const string PaymentIntentKey = "OrchardCore_Commerce_PaymentIntent";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public PaymentIntentPersistence(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Task<string> RetrieveAsync(string key = null)
    {
        var serialized = Session.GetString(PaymentIntentKey);
        if (serialized == null && _httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(PaymentIntentKey, out var serializedCart);
            return Task.FromResult(serializedCart);
        }

        return Task.FromResult(serialized);
    }

    public Task StoreAsync(string paymentIntentId, string key = null)
    {
        if (Session.GetString(PaymentIntentKey) == paymentIntentId) return Task.CompletedTask;

        Session.SetString(PaymentIntentKey, paymentIntentId);
        _httpContextAccessor.SetCookieForever(PaymentIntentKey, paymentIntentId);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key = null)
    {
        Session.Remove(PaymentIntentKey);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(PaymentIntentKey);

        return Task.CompletedTask;
    }
}
