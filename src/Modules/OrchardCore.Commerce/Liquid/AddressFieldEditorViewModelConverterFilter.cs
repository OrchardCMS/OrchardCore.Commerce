using Fluid;
using Fluid.Values;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.Liquid;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class AddressFieldEditorViewModelConverterFilter : ILiquidFilter
{
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input?.ToObjectValue() is not CheckoutViewModel checkoutViewModel) return new ValueTask<FluidValue>(input);

        var cityName = arguments["city_name"].Or(arguments.At(1)).ToStringValue();

        if (arguments["address_field"].Or(arguments.At(0)).ToObjectValue() is not AddressField addressField ||
            string.IsNullOrEmpty(cityName)) return new ValueTask<FluidValue>(input);

        var viewModel = new AddressFieldEditorViewModel
        {
            AddressField = addressField,
            CityName = cityName,
            Regions = checkoutViewModel.Regions,
            Provinces = checkoutViewModel.Provinces,
        };

        return new ValueTask<FluidValue>(new ObjectValue(viewModel));
    }
}
