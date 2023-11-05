using OrchardCore.Modules.Manifest;
using static OrchardCore.Commerce.Payment.Constants.FeatureIds;
using static OrchardCore.Commerce.Promotion.Constants.FeatureIds;

[assembly: Module(
    Name = "Orchard Core Commerce - Payment - Stripe",
    Author = "The Orchard Team",
    Website = "https://github.com/OrchardCMS/OrchardCore.Commerce",
    Version = "0.0.1",
    Description =
        "Stripe payment provider for Orchard Core Commerce. Note: you must configure it in Admin > Configuration > " +
        "Commerce > Stripe API or it won't appear in the front end.",
    Category = "Commerce",
    Dependencies = new[] { Payment, Promotion }
)]
