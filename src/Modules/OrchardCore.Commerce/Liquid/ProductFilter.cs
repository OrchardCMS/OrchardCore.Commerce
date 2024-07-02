using Fluid;
using Fluid.Values;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.Liquid;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Liquid;

public class ProductFilter : ILiquidFilter
{
    private readonly IProductService _productService;

    public ProductFilter(IProductService productService) =>
        _productService = productService;

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (input.Type is not (FluidValues.String or FluidValues.Object)) return NilValue.Instance;

        var sku = input.Type == FluidValues.String
            ? input.ToStringValue()
            : input.ToObjectValue() switch
            {
                OrderLineItem orderLineItem => orderLineItem.ProductSku,
                OrderLineItemViewModel orderLineItemViewModel => orderLineItemViewModel.ProductSku,
                ShoppingCartItem shoppingCartItem => shoppingCartItem.ProductSku,
                ShoppingCartLineUpdateModel shoppingCartLineUpdateModel => shoppingCartLineUpdateModel.ProductSku,
                ShoppingCartLineViewModel shoppingCartLineViewModel => shoppingCartLineViewModel.ProductSku,
                JsonNode jsonString when jsonString.GetValueKind() == JsonValueKind.String => jsonString.Value<string>(),
                JsonObject jsonObject => GetSkuFromJsonObject(jsonObject),
                { } value => value.ToString(),
            };

        return string.IsNullOrWhiteSpace(sku)
            ? NilValue.Instance
            : new ObjectValue((await _productService.GetProductAsync(sku))?.ContentItem);
    }

    private static string GetSkuFromJsonObject(JsonObject jObject)
    {
        var dictionary = jObject.ToObject<Dictionary<string, JsonNode>>().ToDictionaryIgnoreCase();
        if (dictionary.TryGetValue("ProductSku", out var productSku)) return productSku.Value<string>();
        if (dictionary.TryGetValue("Sku", out var sku)) return sku.Value<string>();
        return null;
    }
}
