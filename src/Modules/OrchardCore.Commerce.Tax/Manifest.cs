using OrchardCore.Commerce.Tax.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Core Commerce - Tax",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "Taxation module for Orchard Core Commerce for sales tax or VAT.",
    Category = "Commerce"
)]

[assembly: Feature(
    Id = FeatureIds.Tax,
    Name = "Orchard Core Commerce - Tax",
    Category = "Commerce",
    Description = "Core tax features for Orchard Core Commerce."
)]

[assembly: Feature(
    Id = FeatureIds.CustomTaxRates,
    Name = "Orchard Core Commerce - Custom Tax Rates",
    Category = "Commerce",
    Description = "Enables the admins to locally maintain a set of tax rates.",
    Dependencies = new[]
    {
        FeatureIds.Tax,
    }
)]
