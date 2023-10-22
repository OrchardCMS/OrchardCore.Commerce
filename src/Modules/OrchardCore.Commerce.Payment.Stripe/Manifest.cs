using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Core Commerce - Payment - Stripe",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "Stripe payment provider for Orchard Core Commerce.",
    Category = "Commerce",
    Dependencies = new[] { OrchardCore.Commerce.Payment.Constants.FeatureIds.Payment }
)]
