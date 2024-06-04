using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Commerce.Liquid;

public class OrderLineItemViewModelsAndTaxRatesConverterFilter : ILiquidFilter
{
    private readonly IOrderLineItemService _orderLineItemService;

    public OrderLineItemViewModelsAndTaxRatesConverterFilter(IOrderLineItemService orderLineItemService) =>
        _orderLineItemService = orderLineItemService;

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input?.ToObjectValue() is not { } objectOrderPart)
        {
            return await new ValueTask<FluidValue>(input);
        }

        var orderPart = ((JsonObject)objectOrderPart).ToObject<OrderPart>();

        var viewModels = (await _orderLineItemService
            .CreateOrderLineItemViewModelsAndTotalAsync(orderPart.LineItems, orderPart))
            .ViewModels;

        var viewModelsAndTaxRates = viewModels
            .Select(viewModel =>
                (viewModel, taxRate: viewModel.ProductPart.ContentItem?.As<TaxPart>()?.TaxRate?.Value)).ToList();

        return await new ValueTask<FluidValue>(new ObjectValue(viewModelsAndTaxRates));
    }
}
