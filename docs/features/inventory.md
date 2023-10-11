# Inventory

The local inventory management can help your shop in preventing back orders or managing order quantities. For more details on the `InventoryPart` itself, see its [documentation page](inventory-part.md).

1. Enable the _Orchard Core Commerce - Inventory_ feature.
2. Edit the product in the content item editor.
3. Set the current inventory count.
4. Check the _Allows Back Order_ box if you don't mind back orders and only need this feature to track inventory.
5. Optionally set the minimum and maximum order quantity.
6. You may provide a customized out of stock message, but if you leave it empty, then "Out of Stock" will be displayed anyway.

The inventory count is reduced after the payment is completed, so back orders are still possible. This is preferable to reducing inventory when the product is added to the cart, which could be abused.

Note that the _InventoryPart-AllowsBackOrder_, _InventoryPart-IgnoreInventory_, _InventoryPart-MaximumOrderQuantity_, and _InventoryPart-MinimumOrderQuantity_ shapes are empty by default, to reduce clutter on the product page. If you want to display them, go to Design → Templates in the admin dashboard and create a new template for the one you wish to override.
