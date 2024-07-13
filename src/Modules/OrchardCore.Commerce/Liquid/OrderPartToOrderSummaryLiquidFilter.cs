using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Commerce.Liquid;

public class OrderPartToOrderSummaryLiquidFilter : ILiquidFilter
{
    private readonly IOrderLineItemService _orderLineItemService;

    public OrderPartToOrderSummaryLiquidFilter(IOrderLineItemService orderLineItemService) =>
        _orderLineItemService = orderLineItemService;

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context) =>
        input?.ToObjectValue() is { } objectOrderPart &&
        JObject.FromObject(objectOrderPart).ToObject<OrderPart>() is { } orderPart
            ? ProcessInnerAsync(orderPart)
            : input;

    private async ValueTask<FluidValue> ProcessInnerAsync(OrderPart orderPart)
    {
        var viewModels = (await _orderLineItemService
            .CreateOrderLineItemViewModelsAndTotalAsync(orderPart.LineItems, orderPart))
            .ViewModels;

        if (viewModels == null || viewModels.Count == 0)
        {
            return new ObjectValue(new JsonArray());
        }

        var subTotal = viewModels
            .Select(viewModel => new Amount(
                Round(viewModel.UnitPriceValue, viewModel) * viewModel.Quantity,
                viewModel.UnitPrice.Currency))
            .Sum();
        var total = viewModels.Select(viewModel => viewModel.LinePrice).Sum();

        var expandedViewModels = viewModels
            .Select(viewModel => new
            {
                ViewModel = viewModel,
                TaxRate = viewModel.ProductPart.ContentItem?.As<TaxPart>()?.TaxRate?.Value,
                UnitTax = viewModel.UnitPrice - Round(viewModel.UnitPriceValue, viewModel),
                SubTotal = subTotal,
                TaxTotal = total - subTotal,
                Total = total,
            })
            .ToList();

        return new ObjectValue(JArray.FromObject(expandedViewModels));
    }

    private static decimal Round(decimal value, OrderLineItemViewModel viewModel) =>
        Math.Round(value, viewModel.UnitPrice.Currency.DecimalPlaces);
}
