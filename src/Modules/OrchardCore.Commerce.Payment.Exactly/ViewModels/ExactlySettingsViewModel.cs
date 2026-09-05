using OrchardCore.Commerce.Payment.Exactly.Models;

namespace OrchardCore.Commerce.Payment.Exactly.ViewModels;

public class ExactlySettingsViewModel
{
    public ExactlyEnvironmentSettings Production { get; set; } = new();

    public ExactlyEnvironmentSettings Sandbox { get; set; } = new();
}
