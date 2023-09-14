using OrchardCore.Commerce.Inventory.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Core Commerce - Inventory",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "Inventory management for Orchard Core Commerce.",
    Category = "Commerce"
)]

[assembly: Feature(
    Id = FeatureIds.Inventory,
    Name = "Orchard Core Commerce - Inventory",
    Category = "Commerce",
    Description = "Inventory management for Orchard Core Commerce."
)]
