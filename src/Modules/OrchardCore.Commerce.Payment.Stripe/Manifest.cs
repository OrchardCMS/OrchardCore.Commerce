using OrchardCore.Commerce.Payment.Stripe.Constants;
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
    Dependencies = [Payment, Promotion]
)]

[assembly: Feature(
    Id = FeatureIds.Area,
    Name = "Orchard Core Commerce - Payment - Stripe",
    Category = "Commerce",
    Description =
        "Stripe payment provider for Orchard Core Commerce. Note: you must configure it in Admin > Configuration > " +
        "Commerce > Stripe API or it won't appear in the front end.",
    Dependencies = [Payment, Promotion, FeatureIds.StripeServices]
)]

[assembly: Feature(
    Id = FeatureIds.StripeServices,
    Name = "Orchard Core Commerce - Payment - Stripe - Stripe Services",
    Category = "Commerce",
    Description =
        "Stripe services that are used by Orchard Core Commerce - Payment - Stripe. Note: Added as a feature so it can be mocked in tests."
)]

[assembly: Feature(
    Id = FeatureIds.TestStripeServices,
    Name = "Orchard Core Commerce - Payment - Stripe - Test Stripe Services",
    Category = "Commerce",
    Description =
        "WARNING: Only enable this feature in the UI testing environment." +
        "Simulates Stripe services for testing purposes."
)]
