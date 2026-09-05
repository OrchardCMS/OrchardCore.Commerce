using OrchardCore.Commerce.Payment.Abstractions;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripeApiSettings
{
    public StripeApiEnvironmentSettings Production { get; set; } = new();
    public StripeApiEnvironmentSettings Sandbox { get; set; } = new();

    /// <summary>
    /// Gets or sets the legacy publishable key. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string PublishableKey { get; set; }

    /// <summary>
    /// Gets or sets the legacy secret key. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the legacy account ID. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string AccountId { get; set; }

    /// <summary>
    /// Gets or sets the legacy webhook signing secret. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string WebhookSigningSecret { get; set; }

    public StripeApiEnvironmentSettings Get(PaymentEnvironment environment)
    {
        Production ??= new StripeApiEnvironmentSettings();
        Sandbox ??= new StripeApiEnvironmentSettings();
        MigrateLegacyKeys();

        return environment == PaymentEnvironment.Sandbox ? Sandbox : Production;
    }

    public void MigrateLegacyKeys()
    {
        Production ??= new StripeApiEnvironmentSettings();
        Sandbox ??= new StripeApiEnvironmentSettings();

        if (!string.IsNullOrEmpty(Production.PublishableKey) ||
            !string.IsNullOrEmpty(Production.SecretKey) ||
            string.IsNullOrEmpty(PublishableKey) && string.IsNullOrEmpty(SecretKey))
        {
            return;
        }

        Production.PublishableKey = PublishableKey;
        Production.SecretKey = SecretKey;
        Production.AccountId = AccountId;
        Production.WebhookSigningSecret = WebhookSigningSecret;
    }

    public void ClearLegacyKeys()
    {
        PublishableKey = null;
        SecretKey = null;
        AccountId = null;
        WebhookSigningSecret = null;
    }
}
