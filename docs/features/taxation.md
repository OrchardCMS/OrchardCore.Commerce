# Taxation

Tax support is an ongoing process based on user feedback and contribution. See below what we have right now.

## Core tax support

Enable the _Orchard Core Commerce - Tax_ feature. This grants you the _Tax_ content part, which you can attach to any content type that has a [_Product_ part](products-and-prices.md). This adds the following settings to the content item editor:

- Tax Code: You may use this field to store a product classification code or tax bracket name. This can be used for calculating gross price or for accounting.
- Gross Price & Tax Rate: These are used by the basic tax support, see below.

## Basic tax support

Filling out the _Gross Price_ and _Tax Rate_ (percentage) fields, automatically updates the product's regular price field to the calculated net price during publish. For products configured like this the shopping cart shows the _Gross Price_ instead of the _Price_ field (so it only works with _Price_ part and not the _PriceVariant_ part). This is suitable for stores that only ship locally.

## Locally maintained tax rates

If you only ship to a restricted list of locations, it should be enough to look up the VAT or sales tax rates to those locations. This way, you don't have to subscribe to [an external service](https://github.com/OrchardCMS/OrchardCore.Commerce/issues/159).

1. Enable the _Orchard Core Commerce - Custom Tax Rates_ feature.
2. Go to _Configuration_ → _Commerce_ → _Custom Tax Rates_ in the admin dashboard.
3. You can fill out the City, State, Postal code, Country, and Tax code columns to match against a product.
   - These fields use [regular expressions](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) for more flexibility, though usually you can just treat them as raw text.
   - The state and country codes are the same as the POST values used during checkout, e.g. use US and NY instead of United States and New York respectively.
   - The Address 1 & 2 fields are rarely used, but if you need them for a rule, uncheck the _Hide Address Columns_ checkbox.
   - If a location has multiple taxes, add their rates together. For example Quebec has GST and QST. To handle them together add a row with CA for country, QC for state, and 14.975 for tax rate.
   - Match against the tax code for products with reduced tax rates.
4. Set the tax rate as a numeric percentage.
5. Use the _Add Row_ button to add more rules. These are evaluated from top to bottom, so you can put more specific (e.g. regional) rules above the more generic (e.g. national) ones.
