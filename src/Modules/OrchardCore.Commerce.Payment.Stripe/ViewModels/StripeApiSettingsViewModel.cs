namespace OrchardCore.Commerce.Payment.Stripe.ViewModels;

public class StripeApiSettingsViewModel
{
    public string PublishableKey { get; set; }
    public string SecretKey { get; set; }
    public string AccountId { get; set; }
    public string WebhookSigningSecret { get; set; }
}
