using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
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

        public override IDisplayResult Display(OrderPart orderPart, BuildPartDisplayContext context)
            // TODO: add permissions
            => Initialize<OrderPartViewModel>(GetDisplayShapeType(context), m => BuildViewModel(m, orderPart))
                .Location("Detail", "Content:25")
                .Location("Summary", "Meta:10");

        public override IDisplayResult Edit(OrderPart orderPart, BuildPartEditorContext context)
            => Initialize<OrderPartViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, orderPart));

        public override async Task<IDisplayResult> UpdateAsync(OrderPart orderPart, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(orderPart, Prefix, t => t.LineItems);

            return Edit(orderPart, context);
        }

        private Task BuildViewModel(OrderPartViewModel model, OrderPart part)
            => Task.Run(async () =>
            {
                model.ContentItem = part.ContentItem;
                IDictionary<string, ProductPart> products =
                    await _productService.GetProductDictionary(part.LineItems.Select(line => line.ProductSku));
                OrderLineItemViewModel[] lineItems = await Task.WhenAll(part.LineItems.Select(async lineItem =>
                {
                    ProductPart product = products[lineItem.ProductSku];
                    ContentItemMetadata metaData = await _contentManager.GetContentItemMetadataAsync(product);
                    return new OrderLineItemViewModel
                    {
                        Quantity = lineItem.Quantity,
                        ProductSku = lineItem.ProductSku,
                        ProductName = product.ContentItem.DisplayText,
                        UnitPrice = lineItem.UnitPrice,
                        LinePrice = lineItem.LinePrice,
                        ProductRouteValues = metaData.DisplayRouteValues,
                        Attributes = lineItem.Attributes.ToDictionary(attr => attr.Key, attr =>  attr.Value)
                    };
                }));
                model.LineItems = lineItems;
                model.OrderPart = part;
            });
    }
}
