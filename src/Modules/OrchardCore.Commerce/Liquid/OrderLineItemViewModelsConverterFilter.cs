using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class OrderLineItemViewModelsConverterFilter : ILiquidFilter
{
    private readonly IProductService _productService;
    private readonly IEnumerable<ITaxProvider> _taxProviders;
    private readonly IContentManager _contentManager;

    public OrderLineItemViewModelsConverterFilter(
        IProductService productService,
        IEnumerable<ITaxProvider> taxProviders,
        IContentManager contentManager)
    {
        _productService = productService;
        _taxProviders = taxProviders;
        _contentManager = contentManager;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input?.ToObjectValue() is not IEnumerable<object> objectLineItems) return await new ValueTask<FluidValue>(input);

        var lineItems = new List<OrderLineItem>();

        foreach (var objectLineItem in objectLineItems)
        {
            lineItems.Add(((JObject)objectLineItem).ToObject<OrderLineItem>());
        }

        var viewModel = new List<OrderLineItemViewModel>();

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

        if (_taxProviders.Any())
        {
            var taxContext = new TaxProviderContext(
                viewModelLineItems.Select(item => new TaxProviderContextLineItem(
                    products[item.ProductSku],
                    item.UnitPrice,
                    item.Quantity)),
                new[] { total });

            taxContext = await _taxProviders.UpdateWithFirstApplicableProviderAsync(taxContext);

            foreach (var (item, index) in taxContext.Items.Select((item, index) => (item, index)))
            {
                var lineItem = viewModelLineItems[index];
                lineItem.LinePrice = item.Subtotal;
                lineItem.UnitPrice = item.UnitPrice;
            }
        }

        viewModel.AddRange(viewModelLineItems);

        return await new ValueTask<FluidValue>(new ObjectValue(viewModel));
    }
}
