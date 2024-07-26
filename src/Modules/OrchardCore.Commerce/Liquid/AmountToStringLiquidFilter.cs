using Fluid;
using Fluid.Values;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using OrchardCore.Liquid;
using OrchardCore.Settings;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class AmountToStringLiquidFilter : ILiquidFilter
{
    private readonly ICurrencyProvider _currencyProvider;
    private readonly ISiteService _siteService;

    public AmountToStringLiquidFilter(ICurrencyProvider currencyProvider, ISiteService siteService)
    {
        _currencyProvider = currencyProvider;
        _siteService = siteService;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        Amount amount;

        switch (input.Type)
        {
            case FluidValues.Number:
                var currencyCode = arguments["currency"].Or(arguments.At(1))?.ToStringValue() ??
                                   ((await _siteService.GetSettingsAsync<CurrencySettings>()).CurrentDisplayCurrency;
                var currency = _currencyProvider.GetCurrency(currencyCode);
                amount = new(input.ToNumberValue(), currency);
                break;
            case FluidValues.Object:
                amount = JObject.FromObject(input.ToObjectValue()).ToObject<Amount>();
                break;
            case FluidValues.Array:
            case FluidValues.Blank:
            case FluidValues.Boolean:
            case FluidValues.DateTime:
            case FluidValues.Dictionary:
            case FluidValues.Empty:
            case FluidValues.Function:
            case FluidValues.Nil:
            case FluidValues.String:
            default:
                return input;
        }

        var text = amount.ToString();
        if (arguments["dot"] is { Type: FluidValues.String } dot)
        {
            text = text.Replace(".", dot.ToStringValue());
        }

        return new StringValue(text);
    }
}
