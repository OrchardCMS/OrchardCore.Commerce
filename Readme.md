# Orchard Core Commerce

[![Discord](https://img.shields.io/discord/551136772243980291?color=%237289DA&label=Discord&logo=discord&logoColor=white&style=flat)](https://discord.gg/rYHxgqU5) [![Read the Docs](https://img.shields.io/readthedocs/orchardcorecommerce?label=Documentation)](https://commerce.orchardcore.net/) [![Latest version of 'OrchardCore.Commerce' on NuGet](https://img.shields.io/nuget/v/OrchardCore.Commerce?style=flat&label=NuGet)](https://www.nuget.org/packages/OrchardCore.Commerce/) [![Latest version of 'OrchardCore.Commerce' on Cloudsmith](https://api-prd.cloudsmith.io/v1/badges/version/orchardcore/commerce/nuget/OrchardCore.Commerce/latest/xsp=True/?render=true&show_latest=true&style=flat&labelColor=gray&label=Cloudsmith)](https://cloudsmith.io/~orchardcore/repos/commerce/packages/detail/nuget/OrchardCore.Commerce/latest/xsp=True/) [![Crowdin](https://img.shields.io/badge/Crowdin-translations-lightgrey)](https://crowdin.com/project/orchard-core-commerce)

The commerce module for [Orchard Core](https://github.com/OrchardCMS/OrchardCore).

It's also available on all sites of [DotNest, the Orchard SaaS](https://dotnest.com/).

Do you want to chat with other community members? [Check out our channel on the Orchard discord server.](https://discord.gg/PtaYnX63)

## History, status, and planning

Orchard Core Commerce will be an Orchard Core port and partial rewrite of the open source [Nwazet Commerce](https://github.com/bleroy/Nwazet.Commerce) module that was built for Orchard CMS 1.x. Nwazet Commerce was initially built in 2012 by [Bertrand Le Roy](https://github.com/bleroy), loosely based on [a commerce sample](http://www.ideliverable.com/blog/writing-an-orchard-webshop-module-from-scratch-part-1) by [Sipke Schoorstra](https://github.com/sfmskywalker). The initial goal of Nwazet Commerce was to power the web site of the hardware startup Nwazet. While Nwazet is no longer operating, the Nwazet Commerce project went on, and was further developed by a group of passionate contributors who are using the platform for their own, and their customer's web sites.

Like Orchard, Nwazet Commerce was built with extensibility in mind, and as such it has its own extensions (typical examples include local tax and shipping modules). It's also pure, idiomatic Orchard.

Orchard Core represents a major evolution of the Orchard design principles, and is sufficiently different that running Nwazet Commerce on it will necessitate significant work. As such, we've decided that starting from a blank slate was the best way to go, so we'll port Nwazet Commerce piece by piece, being careful to accurately apply Orchard Core's new design principles. We also decided to adopt a new name, that gets rid of the now obsolete origins, and establishes our ambition for the module to become the go-to commerce module for Orchard Core.

This work is in its initial design phases. There's a lot of work to do, and yes, we do welcome participation in any shape or form. The first stage of this was the [minimum viable feature set](https://github.com/OrchardCMS/OrchardCore.Commerce/milestone/1) which has now been released to NuGet [here](https://www.nuget.org/packages/OrchardCore.Commerce/1.0.0). [Here](https://youtu.be/Sw2jvE82UwE) you can watch a demo video about the MVP.

See the [discussions page](https://github.com/OrchardCMS/OrchardCore.Commerce/discussions) for the latest news and announcements.

## Setting up your dev environment

### Pre-requisites

If you have [Lombiq Analyzers](https://github.com/Lombiq/.NET-Analyzers) included in your project as a submodule you should also add the following property to the _Directory.Build.props_ file:

```xml
<Project>
  <PropertyGroup>
    <LombiqAnalyzersPath>$(MSBuildThisFileDirectory)/tools/Lombiq.Analyzers</LombiqAnalyzersPath>
  </PropertyGroup>
</Project>
```

### Setup

1. Clone this repository.
2. Build and run the `OrchardCore.Commerce.Web` project.
3. Thanks to [Auto Setup](https://docs.orchardcore.net/en/latest/docs/reference/modules/AutoSetup/), the site will be set up with the `OrchardCore Commerce - Development` recipe.
4. Go to the dashboard, using the credentials `admin` and `Password1!`.
5. If you want to test Stripe, go to _Configuration_ → _Commerce_ → _Stripe API_. Set the keys to the test keys found [here](docs/features/stripe-payment.md). If the keys are not set, the Stripe payment button won't appear during checkout.
6. Go to _Content_ → _Content Items_, and create your first `Product`.

## Documentation

Check out the complete documentation portal here: <https://commerce.orchardcore.net/>

- [Inventory](docs/features/inventory.md)
- [Products and Prices](docs/features/products-and-prices.md)
- [Promotions](docs/features/promotions.md)
- [Payment providers](docs/features/payment-providers.md)
  - [Exactly® Payment](docs/features/exactly-payment.md)
  - [Stripe Payment](docs/features/stripe-payment.md)
- [Taxation](docs/features/taxation.md)
- [User Features](docs/features/user-features.md)
- [Workflows](docs/features/workflows.md)

> [!NOTE]
> You can learn more about the translation and localization support for this project in the [OrchardCMS/OrchardCore.Commerce.Translations/](https://github.com/OrchardCMS/OrchardCore.Commerce.Translations/) repository.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues, discussions, and pull requests. Please adhere to our [code of conduct](CODE-OF-CONDUCT.md) while doing so.

You can help translating this project to a language you know, via our [Crowdin project](https://crowdin.com/project/orchard-core-commerce). Check out the [Translations repository](https://github.com/OrchardCMS/OrchardCore.Commerce.Translations/) for more details about it.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.

## Demo video

[![Watch the video](https://img.youtube.com/vi/EVvwS1UaIk4/maxresdefault.jpg)](https://youtu.be/EVvwS1UaIk4)
