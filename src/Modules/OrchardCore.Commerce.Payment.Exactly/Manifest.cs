using OrchardCore.Modules.Manifest;
using static OrchardCore.Commerce.Payment.Constants.FeatureIds;

[assembly: Module(
    Name = "Orchard Core Commerce - Payment - Exactly",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description =
        "Exactly payment provider for Orchard Core Commerce. Note: you must configure it in Admin > Configuration > " +
        "Commerce > Exactly API or it won't appear in the front end.",
    Category = "Commerce",
    Dependencies = [Payment, "OrchardCore.ContentFields.Indexing.SQL"]
)]
