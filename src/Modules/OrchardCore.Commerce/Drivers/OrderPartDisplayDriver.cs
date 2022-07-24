using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class OrderPartDisplayDriver : ContentPartDisplayDriver<OrderPart>
{
    private readonly IProductService _productService;
    private readonly IContentManager _contentManager;

    public OrderPartDisplayDriver(
        IProductService productService,
        IContentManager contentManager)
    {
        _productService = productService;
        _contentManager = contentManager;
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

        for (var i = 0; i < part.LineItems.Count; i++)
        {
            var currentItem = part.LineItems[i];

            var quantity = viewModel.LineItems[i].Quantity;

            if (quantity <= 0)
            {
                part.LineItems.RemoveAt(i);
            }
            else
            {
                currentItem.Quantity = quantity;
                currentItem.LinePrice = quantity * currentItem.UnitPrice;
                part.LineItems[i] = currentItem;
            }
        }

        return await EditAsync(part, context);
    }

    private async ValueTask PopulateViewModelAsync(OrderPartViewModel model, OrderPart part)
    {
        model.ContentItem = part.ContentItem;
        var products = await _productService.GetProductDictionaryAsync(part.LineItems.Select(line => line.ProductSku));
        var lineItems = await Task.WhenAll(part.LineItems.Select(async lineItem =>
        {
            var product = products[lineItem.ProductSku];
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);

            return new OrderLineItemViewModel
            {
                Quantity = lineItem.Quantity,
                ProductSku = lineItem.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPrice = lineItem.UnitPrice,
                LinePrice = lineItem.LinePrice,
                ProductRouteValues = metaData.DisplayRouteValues,
                Attributes = lineItem.Attributes,
            };
        }));

        var total = new Amount();

        foreach (var item in lineItems)
        {
            model.LineItems.Add(item);

            if (total.Currency.Equals(Currency.UnspecifiedCurrency))
            {
                total = item.LinePrice;
            }
            else
            {
                total += item.LinePrice;
            }
        }

        model.Total = total;

        model.Charges.AddRange(part.Charges);

        model.OrderPart = part;
    }
}
