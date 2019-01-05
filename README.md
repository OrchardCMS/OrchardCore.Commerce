# Orchard Core Commerce

[![Join the chat at https://gitter.im/OrchardCore-Commerce/Lobby](https://badges.gitter.im/OrchardCore-Commerce/Lobby.svg)](https://gitter.im/OrchardCore-Commerce/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

The commerce module for [Orchard Core](https://github.com/OrchardCMS/OrchardCore).

## History, status, and planning

Orchard Core Commerce will be an Orchard port and partial rewrite of the open source [Nwazet Commerce](https://github.com/bleroy/Nwazet.Commerce) module that was built for Orchard CMS 1.x. Nwazet Commerce was initially built in 2012 by [Bertrand Le Roy](https://github.com/bleroy), loosely based on [a commerce sample](http://www.ideliverable.com/blog/writing-an-orchard-webshop-module-from-scratch-part-1) by [Sipke Shoorstra](https://github.com/sfmskywalker). The initial goal of Nwazet Commerce was to power the web site of the hardware startup Nwazet. While Nwazet is no longer operating, the Nwazet Commerce project went on, and was further developed by a group of passionate contributors who are using the platform for their own, and their customer's web sites.

Like Orchard, Nwazet Commerce was built with extensibility in mind, and as such it has its own extensions (typical examples include local tax and shipping modules). It's also pure, idiomatic Orchard.

Orchard Core represents a major evolution of the Orchard design principles, and is sufficiently different that running Nwazet Commerce on it will necessitate significant work. As such, we've decided that starting from a blank slate was the best way to go, so we'll port Nwazet Commerce piece by piece, being careful to accurately apply Orchard Core's new design principles. We also decided to adopt a new name, that gets rid of the now obsolete origins, and establishes our ambition for the module to become the go-to commerce module for Orchard Core.

This work is in its initial design phases. There's a lot of work to do, and yes, we do welcome participation in any shape or form.

The work will focus at first on porting a [minimum viable feature set](https://github.com/OrchardCMS/OrchardCore.Commerce/issues/3):

* Product, price, and inventory parts and/or fields (those were a single part in Nwazet)
  Note: not all products have a price
* Shopping cart
* Checkout (probably redesigned around Orchard Workflows)
* Base infrastructure for payment, plus one implementation (Stripe)
* Order content type and management screens (including a redesign and refactoring of the order part)
* Workflow activities

Globalization should be taken into account at every step.

## Setting up your dev environment

1. **Prerequisites:** Make sure you have an up-to-date clone of [the Orchard Core repository](https://github.com/OrchardCMS/OrchardCore) on the `dev` branch. Please consult [the Orchard Core documentation](https://orchardcore.readthedocs.io/en/latest/) and make sure you have a working Orchard before you proceed. You'll also, of course, need all of Orchard Core's prerequisites for development (.NET Core, a code editor, etc.). The following steps assume some basic understanding of Orchard Core.
2. Clone the module under `[your Orchard Core clone's root]/src/OrchardCore.Modules`.
3. Add the existing project to the solution under `src/OrchardCore.Modules` in the solution explorer if you're using Visual Studio.
4. Add a reference to the module from the `OrchardCore.Cms.Web` project.
5. Build, run.
6. From the admin, enable the module's only feature.
7. (optional) Start using the features, by creating a new `Product` content type, and adding the product part to it.
