using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Modules.Manifest;
using static OrchardCore.Commerce.CommerceConstants.Features;
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
    Dependencies = [Payment, Promotion]
)]

[assembly: Feature(
    Id = FeatureIds.Area,
    Name = "Orchard Core Commerce - Payment - Stripe",
    Description =
        "Stripe payment provider for Orchard Core Commerce. Note: you must configure it in Admin > Configuration > " +
        "Commerce > Stripe API or it won't appear in the front end.",
    Category = "Commerce",
    Dependencies = [Payment, Promotion, Subscription]
)]

[assembly: Feature(
    Id = FeatureIds.DummyStripeServices,
    Name = "Orchard Core Commerce - Payment - Stripe - Dummy Stripe Services",
    Category = "Commerce",
    Description =
        "WARNING: Only enable this feature in the UI testing environment." +
        "Simulates Stripe services for testing purposes.",
    Dependencies = [FeatureIds.Area]
)]
