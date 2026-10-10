namespace OrchardCore.Commerce.Abstractions.Constants;

public static class ConfigurationConstants
{
    public const string Commerce = nameof(Commerce);
    public const string HeadersDisplayNames = nameof(HeadersDisplayNames);
    public const string HeadersConfig = $"{Commerce}:{HeadersDisplayNames}";
    public const string Payment = nameof(Payment);
    public const string Exactly = nameof(Exactly);
    public const string PaymentExactly = nameof(PaymentExactly);
    public const string PaymentExactlyConfig = $"{Commerce}:{PaymentExactly}";
    public const string OldHeadersConfig = "OrchardCoreCommerce_HeadersDisplayNames";
    public const string OldPaymentExactlyConfig = "OrchardCoreCommerce_Payment_Exactly";
}
