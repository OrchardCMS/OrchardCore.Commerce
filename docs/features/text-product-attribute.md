# TextProductAttributeField

Allows adding further details to a product. When used in combination with a [_PriceVariantsPart_](price-variants-part.md), it creates variants for the product. 

## Fields and properties
- **Hint** (`string`): Sets the description text to display for this attribute on the product's page.
- **Required** (`bool`): Determines whether a value is required.
- **DefaultValue** (`T`): Sets the default value.
- **Placeholder** (`string`): Sets the hint to display when the input is empty.
- **PredefinedValues** (`IEnumerable<object>`): Holds the set of allowed values. These are also used to create variants with a _PriceVariantsPart_.
- **RestrictToPredefinedValues** (`bool`): Determines whether values should be restricted to the set of predefined values. Note that this must be set to true for _PriceVariantsPart_ to pick up the values correctly.
- **MultipleValues** (`bool`): Determines whether multiple values can be selected.

## Usage examples
New attribute fields can be added or existing fields can be edited in the relevant product content type's editor.
![image](../assets/images/text-product-attribute/content-type-editor-example.png)

![image](../assets/images/text-product-attribute/attribute-field-editor-example.png)

The predefined values of the attribute are displayed on the product's page.
![image](../assets/images/text-product-attribute/attribute-field-display-example.png)
