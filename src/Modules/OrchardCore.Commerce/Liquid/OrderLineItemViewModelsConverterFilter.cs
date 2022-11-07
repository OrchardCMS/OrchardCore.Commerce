using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Helpers;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using System.Collections.Generic;
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
        if (input?.ToObjectValue() is not IEnumerable<object> objectLineItems)
        {
            return await new ValueTask<FluidValue>(input);
        }

        var lineItems = new List<OrderLineItem>();

        foreach (var objectLineItem in objectLineItems)
        {
            lineItems.Add(((JObject)objectLineItem).ToObject<OrderLineItem>());
        }

        var viewModels = (await OrderLineItemHelpers
            .CreateOrderLineItemViewModelsAndTotalAsync(lineItems, _contentManager, _productService, _taxProviders))
            .ViewModels;

        return await new ValueTask<FluidValue>(new ObjectValue(viewModels));
    }
}
