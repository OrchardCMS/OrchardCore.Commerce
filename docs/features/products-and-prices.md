# Products and Prices

To create a content type that represents a sellable product, you must give it a _Product_ part and either _Price_ or _PriceVariant_ part as well. You may have multiple different content types that represent different classes of product but they all must follow this rule.

> ℹ Use the _Orchard Core Commerce - Content - Product_ recipe to set up a fully featured Product content type.

- [ProductPart](product-part.md): contains the product's stock keeping unit (SKU) which has to be globally unique.
- [PricePart](price-part.md): contains a single price for the product.
- [PriceVariantsPart](price-variants-part.md): contains multiple prices. This part uses any _Text Product Attribute Field_ the content type has, where the _Restrict to predefined values_ checkbox is checked in the field's settings. If you have multiple such fields, it creates a price field for each possible combination. For example if you have Size (S; M; L) and Color (Red; Blue) then you can give separate prices for Small Red, Medium Blue, etc.
- [TieredPricePart](tiered-price-part.md): contains multiple prices based on selected quantity.

You can add fields to a product content type, the buyer can use these to enter further details for their order:

- [Boolean Product Attribute Field](boolean-product-attribute.md)
- [Numeric Product Attribute Field](numeric-product-attribute.md)
- [Text Product Attribute Field](text-pproduct-attribute.md)

There are more optional details regarding prices, see the [Taxation](taxation.md) and [Promotions](promotions.md) pages.
