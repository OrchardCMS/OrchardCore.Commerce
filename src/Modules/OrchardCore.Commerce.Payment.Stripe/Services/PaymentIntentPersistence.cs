using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Services;

public class PaymentIntentPersistence : IPaymentIntentPersistence
{
    private const string PaymentIntentKey = "OrchardCore:Commerce:PaymentIntent";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private ISession Session => _httpContextAccessor.HttpContext?.Session;

    public PaymentIntentPersistence(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string Retrieve() => Session.GetString(PaymentIntentKey);

    public void Store(string paymentIntentId) => Session.SetString(PaymentIntentKey, paymentIntentId);

    public void Remove() => Session.Remove(PaymentIntentKey);
}
