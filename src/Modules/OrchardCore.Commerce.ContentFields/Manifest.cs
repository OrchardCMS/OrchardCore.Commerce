using OrchardCore.Modules.Manifest;
using static OrchardCore.Commerce.ContentFields.Constants.FeatureIds;

[assembly: Module(
    Name = "Orchard Core Commerce - Content Fields",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description = "Commerce-specific content fields for Orchard Core.",
    Category = "Commerce"
)]

[assembly: Feature(
    Id = ContentFields,
    Name = "Orchard Core Commerce - Content Fields",
    Category = "Commerce",
    Description = "Commerce-specific content fields for Orchard Core.",
    Dependencies =
    [
        "OrchardCore.ContentFields",
    ]
)]

[assembly: Feature(
    Id = WesternNameParts,
    Name = "Orchard Core Commerce - Western Name Parts",
    Category = "Commerce",
    Description = "Enabling this feature provides an address updater and shape override for the address editor that " +
                  "implements common name parts in English-speaking and many other western cultures. This will break " +
                  "up the Name field into Title, Given Name, Middle Name and Family Name fields.",
    Dependencies =
    [
        ContentFields,
    ]
)]
