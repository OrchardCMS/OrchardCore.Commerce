namespace OrchardCore.Commerce.Payment.Abstractions;

/// <summary>
/// Distinguishes live (production) payment credentials from sandbox/test credentials.
/// </summary>
public enum PaymentEnvironment
{
    Production,
    Sandbox,
}
