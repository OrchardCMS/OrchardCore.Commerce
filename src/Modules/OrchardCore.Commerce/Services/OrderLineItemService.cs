using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class OrderLineItemService : IOrderLineItemService
{
    private readonly IProductService _productService;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<ITaxProvider> _taxProviders;
    private readonly IPromotionService _promotionService;

    public OrderLineItemService(
        IProductService productService,
        IContentManager contentManager,
        IEnumerable<ITaxProvider> taxProviders,
        IPromotionService promotionService)
    {
        _productService = productService;
        _contentManager = contentManager;
        _taxProviders = taxProviders;
        _promotionService = promotionService;
    }

    public async Task<(IList<OrderLineItemViewModel> ViewModels, Amount Total)> CreateOrderLineItemViewModelsAndTotalAsync(
        IList<OrderLineItem> lineItems,
        DateTime? publishDateTime = null)
    {
        var products = await _productService.GetProductDictionaryByContentItemVersionsAsync(
            lineItems.Select(line => line.ContentItemVersion));

        var viewModelLineItems = await Task.WhenAll(lineItems.Select(async lineItem =>
        {
            var product = products[lineItem.ProductSku];
            var metaData = await _contentManager.GetContentItemMetadataAsync(product);
            return new OrderLineItemViewModel
            {
                ProductPart = product,
                Quantity = lineItem.Quantity,
                ProductSku = lineItem.ProductSku,
                ProductName = product.ContentItem.DisplayText,
                UnitPrice = lineItem.UnitPrice,
                LinePrice = lineItem.LinePrice,
                ProductRouteValues = metaData.DisplayRouteValues,
                Attributes = lineItem.Attributes,
            };
        }));

        var promotionAndTaxContext = new PromotionAndTaxProviderContext(
            viewModelLineItems.Select(item => new PromotionAndTaxProviderContextLineItem(
                products[item.ProductSku],
                item.UnitPrice,
                item.Quantity)),
            viewModelLineItems.CalculateTotals().ToList(),
            publishDateTime);
        var changed = false;

        if (_taxProviders.Any() &&
            await _taxProviders.GetFirstApplicableProviderAsync(promotionAndTaxContext) is { } taxProvider)
        {
            promotionAndTaxContext = await taxProvider.UpdateAsync(promotionAndTaxContext);
            changed = true;
        }

        if (await _promotionService.IsThereAnyApplicableProviderAsync(promotionAndTaxContext))
        {
            promotionAndTaxContext = await _promotionService.AddPromotionsAsync(promotionAndTaxContext);
            changed = true;
        }

        if (changed)
        {
            foreach (var (item, index) in promotionAndTaxContext.Items.Select((item, index) => (item, index)))
            {
                var lineItem = viewModelLineItems[index];
                lineItem.LinePrice = item.Subtotal;
                lineItem.UnitPrice = item.UnitPrice;
            }
        }

        return (viewModelLineItems, viewModelLineItems.CalculateTotals().Single());
    }
}
