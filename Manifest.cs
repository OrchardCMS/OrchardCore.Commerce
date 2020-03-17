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
    Description = "Registers the core components used by the Commerce features.",
    Dependencies = new[]
    {
        "OrchardCore.Contents"
    }
)]

[assembly: Feature(
    Id = CommerceConstants.Features.SessionCartStorage,
    Name = "Orchard Core Commerce Session Cart Storage",
    Category = "Commerce",
    Description = "Registers session-based shopping cart persistence.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        CommerceConstants.Features.Core
    }
)]
[assembly: Feature(
    Id = CommerceConstants.Features.CommerceSettingsCurrencySelector,
    Name = "Orchard Core Commerce Settings Currency Selector",
    Category = "Commerce",
    Description = "Currency selector that uses display currency configured in settings. Useful for Dev/Test scenarios.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        CommerceConstants.Features.Core
    }
)]

[assembly: Feature(
    Id = CommerceConstants.Features.PriceBooks,
    Name = "Orchard Core Commerce Price Books",
    Category = "Commerce",
    Description = "Provides core price book functionality allowing for multiple prices per product.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        "OrchardCore.Lists",
        "OrchardCore.Admin",
        CommerceConstants.Features.Core
    }
)]

[assembly: Feature(
    Id = CommerceConstants.Features.PriceBooksByUser,
    Name = "Orchard Core Commerce Price Books by User",
    Category = "Commerce",
    Description = "Associates a price book with a user.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        CommerceConstants.Features.Core,
        CommerceConstants.Features.PriceBooks
    }
)]

[assembly: Feature(
    Id = CommerceConstants.Features.PriceBooksByRole,
    Name = "Orchard Core Commerce Price Books by Role",
    Category = "Commerce",
    Description = "Associates a price book with a user via roles.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        "OrchardCore.Security",
        CommerceConstants.Features.Core,
        CommerceConstants.Features.PriceBooks
    }
)]
