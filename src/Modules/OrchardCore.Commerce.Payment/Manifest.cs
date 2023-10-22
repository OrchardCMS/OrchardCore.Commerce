using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Core Commerce - Payment",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "Payment for Orchard Core Commerce.",
    Category = "Commerce"
)]

[assembly: Feature(
    Id = FeatureIds.Payment,
    Name = "Orchard Core Commerce - Payment",
    Category = "Commerce",
    Description = "Payment for Orchard Core Commerce."
)]

[assembly: Feature(
    Id = FeatureIds.Stripe,
    Name = "Orchard Core Commerce - Payment - Stripe",
    Category = "Commerce",
    Description = "Stripe payment provider.",
    Dependencies = new[] { FeatureIds.Payment }
)]

[assembly: Feature(
    Id = FeatureIds.DummyProvider,
    Name = "Orchard Core Commerce - Payment - Dummy Provider",
    Category = "Commerce",
    Description = "Dummy payment provider used for development and testing.",
    Dependencies = new[] { FeatureIds.Payment }
)]
