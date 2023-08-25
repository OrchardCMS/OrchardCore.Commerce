# Workflows

## Shopping Cart Event Workflow Events

These events get triggered by `WorkflowShoppingCartEvents` which implements the `IShoppingCartEvents` interface. For each you can access the input as a .NET object using the `Context` workflow input and the serialized version as the `JSON` workflow input.
All of these workflows expect to return one or more outputs which is passed back to the invoking code.

> ⚠ If you want to return an altered version of the input as the output, please always use the JSON which is already serialized in the expected format used by OrchardCore.Commerce's converters. For example, you can use the JS expression `JSON.parse(input('JSON'))`.

> ℹ When your output contains `LocalizedHtmlString`, it can be represented in JS either as `string` or `{ Name: string, Value: string }`. In case of just `string` the same text becomes `LocalizedHtmlString.Name` and `LocalizedHtmlString.Value` too.

### "Cart displaying" Event

Executes after the shopping cart data is prepared, but before the shapes are rendered.

- Input: `ShoppingCartDisplayingEventContext` object containing the current shopping cart's headers and lines.
- Outputs: either outputs are optional.
  - Headers: `LocalizedHtmlString` array. The shopping cart header labels in order. If you have to support multiple locales, make sure to use the object format mentioned above, because `LocalizedHtmlString.Name` is used to generate the template name for the corresponding shopping cart column's cells.
  - Lines: `ShoppingCartLineViewModel` array. This is only for display, in most cases, you shouldn't have to return this output.

### "Verifying cart item" Event

Executes before an item is added to the shopping cart to check whether it can be added based on inventory status.

- Input: `ShoppingCartItem` object.
- Outputs:
  - Error: `LocalizedHtmlString` or `null`. The error message to be displayed if the input item can't be added to the cart. You can simply not output anything if the validation passes.

### "Cart loaded" Event

Executes after the shopping cart content is loaded from the store and before it's displayed or used for calculation.

- Input: `ShoppingCart` object.
- Outputs:
  - ShoppingCart: `ShoppingCart` object. An altered version of the input. If no changes are necessary, the output can be skipped. Here it's the most important to only use `input('JSON')` as mentioned above, because `ShoppingCart` has custom JSON converters inside that will ony correctly serialize in .NET code.

## Other Workflow Events

These events are triggered without an expectation of an output. They can be used for other automation.

### "Product added to cart" Event

Executes when a product is added to the shopping cart.

- Inputs:
  - LineItem: `ShoppingCartItem` object.

### "Order was created" Event

Executes when an order is created on the frontend.

- Inputs:
  - ContentItem: `ContentItem` object of the `Order` content item that has just been created.
