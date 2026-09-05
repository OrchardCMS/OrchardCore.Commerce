namespace OrchardCore.Commerce.Payment.Stripe.ViewModels;

public class StripeApiSettingsViewModel
{
    public StripeApiEnvironmentSettingsViewModel Production { get; set; } = new();
    public StripeApiEnvironmentSettingsViewModel Sandbox { get; set; } = new();
    public string ProductionWebhookUrl { get; set; }
    public string SandboxWebhookUrl { get; set; }
}
