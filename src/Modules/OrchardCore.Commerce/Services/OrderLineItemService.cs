using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
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
    IList<OrderLineItem> lineItems)
    {
        var products = await _productService.GetProductDictionaryAsync(lineItems.Select(line => line.ProductSku));
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

        var total = viewModelLineItems.Select(item => item.LinePrice).Sum();

        var taxAndPromotionContext = new PromotionAndTaxProviderContext(
            viewModelLineItems.Select(item => new PromotionAndTaxProviderContextLineItem(
                products[item.ProductSku],
                item.UnitPrice,
                item.Quantity)),
            new[] { total });

        if (_taxProviders.Any())
        {
            taxAndPromotionContext = await _taxProviders.UpdateWithFirstApplicableProviderAsync(taxAndPromotionContext);
        }

        taxAndPromotionContext = await _promotionService.AddPromotionsAsync(taxAndPromotionContext);

        foreach (var (item, index) in taxAndPromotionContext.Items.Select((item, index) => (item, index)))
        {
            var lineItem = viewModelLineItems[index];
            lineItem.LinePrice = item.Subtotal;
            lineItem.UnitPrice = item.UnitPrice;
        }

        total = taxAndPromotionContext.TotalsByCurrency.Single();

        return (viewModelLineItems, total);
    }
}
