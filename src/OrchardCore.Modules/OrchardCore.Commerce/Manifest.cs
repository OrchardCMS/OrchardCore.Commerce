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
    Name = "Orchard Core Commerce - Core",
    Category = "Commerce",
    Description = "Registers the core components used by the Commerce features.",
    Dependencies =
    [
        "OrchardCore.Contents",
        "OrchardCore.Workflows",
        "OrchardCore.Templates",
        "OrchardCore.Commerce.ContentFields",
        "OrchardCore.Commerce.Payment",
    ]
)]

[assembly: Feature(
    Id = CommerceConstants.Features.SessionCartStorage,
    Name = "Orchard Core Commerce - Session Cart Storage",
    Category = "Commerce",
    Description = "Registers session-based shopping cart persistence.",
    Dependencies =
    [
        "OrchardCore.Contents",
        CommerceConstants.Features.Core,
    ]
)]

[assembly: Feature(
    Id = CommerceConstants.Features.CurrencySettingsSelector,
    Name = "Orchard Core Commerce - Currency Settings Selector",
    Category = "Commerce",
    Description = "Currency selector that uses display currency configured in settings. Useful for Dev/Test scenarios.",
    Dependencies =
    [
        "OrchardCore.Contents",
        CommerceConstants.Features.Core,
    ]
)]

[assembly: Feature(
    Id = CommerceConstants.Features.Subscription,
    Name = "Orchard Core Commerce - Subscription",
    Category = "Commerce",
    Description = "Subscription management. Currently only supports Stripe.",
    Dependencies =
    [
        "OrchardCore.Contents",
        CommerceConstants.Features.Core,
    ]
)]
