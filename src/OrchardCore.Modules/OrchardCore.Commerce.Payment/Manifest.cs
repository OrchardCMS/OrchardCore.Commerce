using OrchardCore.Modules.Manifest;
using static OrchardCore.Commerce.ContentFields.Constants.FeatureIds;
using static OrchardCore.Commerce.Payment.Constants.FeatureIds;
using static OrchardCore.Commerce.Tax.Constants.FeatureIds;

[assembly: Module(
    Name = "Orchard Core Commerce - Payment",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "Payment for Orchard Core Commerce.",
    Category = "Commerce"
)]

[assembly: Feature(
    Id = Payment,
    Name = "Orchard Core Commerce - Payment",
    Category = "Commerce",
    Description = "Payment for Orchard Core Commerce.",
    Dependencies = [ContentFields, Tax]
)]

[assembly: Feature(
    Id = DummyProvider,
    Name = "Orchard Core Commerce - Payment - Dummy Provider",
    Category = "Commerce",
    Description = "Dummy payment provider used for development and testing.",
    Dependencies = [Payment]
)]
