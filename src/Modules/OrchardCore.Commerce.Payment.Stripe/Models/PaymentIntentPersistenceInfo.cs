using OrchardCore.Commerce.MoneyDataType;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public record PaymentIntentPersistenceInfo(string PaymentIntentId, Amount Amount);
