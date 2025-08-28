namespace OrchardCore.Commerce.Payment.Constants;

public static class FeatureIds
{
    public const string Area = "OrchardCore.Commerce.Payment";

    public const string Payment = Area;
    public const string DummyProvider = $"{Area}.{nameof(DummyProvider)}";
    public const string WithoutPaymentProvider = $"{Area}.{nameof(WithoutPaymentProvider)}";
    public const string NoNecessaryPaymentProvider = $"{Area}.{nameof(NoNecessaryPaymentProvider)}";
}
