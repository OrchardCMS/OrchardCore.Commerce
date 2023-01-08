using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Liquid;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class OrderLineItemViewModelsConverterFilter : ILiquidFilter
{
    private readonly IOrderLineItemService _orderLineItemService;

    public OrderLineItemViewModelsConverterFilter(IOrderLineItemService orderLineItemService) =>
        _orderLineItemService = orderLineItemService;

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input?.ToObjectValue() is not IEnumerable<object> objectLineItems)
        {
            return await new ValueTask<FluidValue>(input);
        }

        var lineItems = objectLineItems
            .Select(objectLineItem => ((JObject)objectLineItem).ToObject<OrderLineItem>())
            .ToList();

        var viewModels = (await _orderLineItemService
            .CreateOrderLineItemViewModelsAndTotalAsync(lineItems, orderPart: null))
            .ViewModels;

        return await new ValueTask<FluidValue>(new ObjectValue(viewModels));
    }
}
