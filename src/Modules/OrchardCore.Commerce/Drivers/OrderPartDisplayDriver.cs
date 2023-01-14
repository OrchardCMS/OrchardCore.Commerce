using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class OrderPartDisplayDriver : ContentPartDisplayDriver<OrderPart>
{
    private readonly IOrderLineItemService _orderLineItemService;

    public OrderPartDisplayDriver(IOrderLineItemService orderLineItemService) =>
        _orderLineItemService = orderLineItemService;

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

        var lineItems = part.LineItems
             .Where(lineItem => lineItem != null)
             .Select((lineItem, index) =>
             {
                 var quantity = viewModel.LineItems[index].Quantity;
                 lineItem.Quantity = quantity;
                 lineItem.LinePrice = quantity * lineItem.UnitPrice;

                 return lineItem;
             })
             .Where(lineItem => lineItem.Quantity != 0)
             .ToList();

        part.LineItems.Clear();
        part.LineItems.AddRange(lineItems);

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
