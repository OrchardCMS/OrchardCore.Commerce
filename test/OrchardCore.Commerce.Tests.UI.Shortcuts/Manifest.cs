using OrchardCore.Modules.Manifest;
using static OrchardCore.Commerce.Tests.UI.Shortcuts.ShortcutsFeatureIds;

[assembly: Module(
    Name = "Shortcuts - Orchard Core Commerce",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1"
)]

[assembly: Feature(
    Id = Default,
    Name = "Orchard Core Commerce UI Test - Shortcuts",
    Category = "Development",
    Description = "WARNING: Only enable this feature in the UI testing environment. Provides shortcuts for Orchard " +
        "Core Commerce operations that UI tests might want to do or check.",
    Dependencies = new[]
    {
        "OrchardCore.ContentManagement",
    }
)]
