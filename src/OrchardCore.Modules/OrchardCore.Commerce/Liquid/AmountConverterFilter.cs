using Fluid;
using Fluid.Values;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Liquid;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class AmountConverterFilter : ILiquidFilter
{
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context) =>
        input is { Type: FluidValues.Object }
            ? new(new ObjectValue(JObject.FromObject(input.ToObjectValue()).ToObject<Amount>()))
            : new(input);
}
