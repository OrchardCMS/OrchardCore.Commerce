using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class OrderPartDisplayDriver : ContentPartDisplayDriver<OrderPart>
{
    private readonly IOrderLineItemService _orderLineItemService;
    private readonly ICurrencyProvider _currencyProvider;
    private readonly IProductService _productService;

    public OrderPartDisplayDriver(
        IOrderLineItemService orderLineItemService,
        ICurrencyProvider currencyProvider,
        IProductService productService)
    {
        _orderLineItemService = orderLineItemService;
        _currencyProvider = currencyProvider;
        _productService = productService;
    }

    public override IDisplayResult Display(OrderPart part, BuildPartDisplayContext context) =>
        Initialize<OrderPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModelAsync(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(OrderPart part, BuildPartEditorContext context) =>
        Initialize<OrderPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModelAsync(viewModel, part));

    public override async Task<IDisplayResult> UpdateAsync(OrderPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new OrderPartViewModel();
        await updater.TryUpdateModelAsync(viewModel, Prefix);

        var viewModelLineItems = viewModel.LineItems
            .Where(lineItem => lineItem != null)
            .Select((lineItem, _) =>
            {
                var lineItemCurrency = _currencyProvider.GetCurrency(lineItem.UnitPriceCurrencyIsoCode);

                lineItem.UnitPrice = new Amount(lineItem.UnitPriceValue, lineItemCurrency);
                lineItem.LinePrice = lineItem.UnitPrice * lineItem.Quantity;

                return lineItem;
            })
            .Where(lineItem => lineItem.Quantity != 0)
            .ToList();

        // convert above line items to OrderLineItems and add them to part
        var orderLineItems = new List<OrderLineItem>();
        foreach (var lineItem in viewModelLineItems)
        {
            //var productPart = await _productService.GetProductAsync(lineItem.ProductSku);
            // If the provided SKU does not belong to an existing product content item, it should not be added.
            if (await _productService.GetProductAsync(lineItem.ProductSku.ToUpperInvariant()) is not { } productPart)
            {
                continue;
            }

            orderLineItems.Add(new OrderLineItem(
                lineItem.Quantity,
                lineItem.ProductSku.ToUpperInvariant(),
                lineItem.UnitPrice,
                lineItem.LinePrice,
                productPart.ContentItem.ContentItemVersionId
            ));
        }

        //var lineItems = part.LineItems // use viewModel's LineItems instead of part's LineItems to get the new list
        //     .Where(lineItem => lineItem != null)
        //     .Select((lineItem, index) =>
        //     {
        //         var quantity = viewModel.LineItems[index].Quantity;
        //         lineItem.Quantity = quantity;
        //         lineItem.LinePrice = quantity * lineItem.UnitPrice;

        //         return lineItem;
        //     })
        //     .Where(lineItem => lineItem.Quantity != 0)
        //     .ToList();

        part.LineItems.SetItems(orderLineItems);

        return await EditAsync(part, context);
    }

    private async ValueTask PopulateViewModelAsync(OrderPartViewModel model, OrderPart part)
    {
        model.ContentItem = part.ContentItem;
        var lineItems = part.LineItems;
        var lineItemViewModelsAndTotal = await _orderLineItemService
            .CreateOrderLineItemViewModelsAndTotalAsync(lineItems, part);

        model.Total = lineItemViewModelsAndTotal.Total;
        model.LineItems.AddRange(lineItemViewModelsAndTotal.ViewModels);
        model.Charges.AddRange(part.Charges);
        model.OrderPart = part;
    }
}
