using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Liquid;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class AmountConverterFilter : ILiquidFilter
{
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input.Type == FluidValues.Nil ||
            input.ToObjectValue() is not JToken newInput)
        {
            return new ValueTask<FluidValue>(input);
        }

        var newObject = newInput.ToObject<Amount>();
        return new ValueTask<FluidValue>(new ObjectValue(newObject));
    }
}
