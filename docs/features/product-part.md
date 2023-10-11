# ProductPart

A content item becomes a product if it has a _ProductPart_ and a price providing part (e.g. [_PricePart_](price-part.md), [_PriceVariantsPart_](price-variants-part.md), or [TieredPricePartPart_](tiered-price-part.md)).

## Fields and properties
- **SKU** (`string`): The product's stock keeping unit, used for identification purposes. Must be globally unique and cannot contain the hyphen (`-`) character.
- **CanBeBought** (`IDictionary<string, bool>`): Determines whether the product can currently be bought based on current inventory settings. If there is no [_InventoryPart_](inventory-part.md) on the product, it is unused. This is not editable in the product's editor.
- **ProductImage** (`MediaField`): Allows selecting an image from the Media Library that will be displayed on the product's page.

## Usage examples
The _SKU_ and the _Product Image_ properties can be set in the product's editor.
![image](../assets/images/product-part/product-editor-example.png)

If _Product Image_ is set, it will appear on the product's page.
![image](../assets/images/product-part/product-image-example.png)
