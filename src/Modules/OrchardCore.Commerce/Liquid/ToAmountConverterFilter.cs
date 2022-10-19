using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Liquid;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class ToAmountConverterFilter : ILiquidFilter
{
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input.Type == FluidValues.Nil) return new ValueTask<FluidValue>(input);

        var newObject = (input.ToObjectValue() as JToken).ToObject<Amount>();
        return new ValueTask<FluidValue>(new ObjectValue(newObject));
    }
}
