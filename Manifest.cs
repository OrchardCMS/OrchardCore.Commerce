using OrchardCore.Commerce;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrchardCore.Commerce",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "The commerce module for Orchard Core."
)]

[assembly: Feature(
    Id = CommerceConstants.Features.Core,
    Name = "Orchard Core Commerce",
    Category = "Commerce",
    Description = "Registers the core components used by the Commerce features."
)]
