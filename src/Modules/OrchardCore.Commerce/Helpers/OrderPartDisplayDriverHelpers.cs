using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Helpers;

public static class OrderPartDisplayDriverHelpers
{
    public static async ValueTask PopulateViewModelAsync(
        OrderPartViewModel model,
        OrderPart part,
        IProductService productService,
        IContentManager contentManager)
    {
        model.ContentItem = part.ContentItem;
        var products = await productService.GetProductDictionaryAsync(part.LineItems.Select(line => line.ProductSku));
        var lineItems = await Task.WhenAll(part.LineItems.Select(async lineItem =>
        {
            var product = products[lineItem.ProductSku];
            var metaData = await contentManager.GetContentItemMetadataAsync(product);

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

        if (lineItems.Any())
        {
            total = new Amount(0, lineItems.FirstOrDefault().LinePrice.Currency);

            foreach (var item in lineItems)
            {
                model.LineItems.Add(item);

                total += item.LinePrice;
            }
        }

        model.Total = total;

        model.Charges.AddRange(part.Charges);

        model.OrderPart = part;
    }
}
