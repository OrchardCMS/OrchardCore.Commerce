using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class PaymentIntentPersistence : IPaymentIntentPersistence
{
    // Using _ as a separator to avoid separator character conflicts.
    private const string PaymentIntentKey = "OrchardCore_Commerce_PaymentIntent";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public PaymentIntentPersistence(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string Retrieve()
    {
        var serialized = Session.GetString(PaymentIntentKey);
        if (serialized == null && _httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(PaymentIntentKey, out var serializedCart);
            return serializedCart;
        }

        return serialized;
    }

    public void Store(string paymentIntentId)
    {
        if (Session.GetString(PaymentIntentKey) == paymentIntentId) return;

        Session.SetString(PaymentIntentKey, paymentIntentId);
        _httpContextAccessor.SetCookieForever(PaymentIntentKey, paymentIntentId);
    }

    public void Remove()
    {
        Session.Remove(PaymentIntentKey);
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(PaymentIntentKey);
    }
}
