using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ProductAttributeValues;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ShoppingCartSerializer : IShoppingCartSerializer
{
    private readonly IEnumerable<IProductAttributeProvider> _attributeProviders;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IMoneyService _moneyService;
    private readonly IProductService _productService;

    public ShoppingCartSerializer(
        IEnumerable<IProductAttributeProvider> attributeProviders,
        IContentDefinitionManager contentDefinitionManager,
        IMoneyService moneyService,
        IProductService productService)
    {
        _attributeProviders = attributeProviders;
        _contentDefinitionManager = contentDefinitionManager;
        _moneyService = moneyService;
        _productService = productService;
    }

    public Task<string> SerializeAsync(ShoppingCart cart) => Task.FromResult(JsonSerializer.Serialize(cart));

    public async Task<ShoppingCart> ParseCartAsync(ShoppingCartUpdateModel cart)
    {
        var products = await GetProductsAsync(cart.Lines.Select(line => line.ProductSku));
        var types = await ExtractTypeDefinitionsAsync(products.Values);

        return new ShoppingCart(cart.Lines
            .Where(updateModel => updateModel.Quantity > 0)
            .Select(updateModel => new ShoppingCartItem(
                updateModel.Quantity,
                updateModel.ProductSku,
                ParseAttributes(updateModel, types[products[updateModel.ProductSku].ContentItem.ContentType]))));
    }

    public async Task<ShoppingCart> DeserializeAsync(string serializedCart) =>
        (await DeserializeAndVerifyAsync(serializedCart)).ShoppingCart;

    public async Task<DeserializeResult> DeserializeAndVerifyAsync(string serializedCart)
    {
        if (string.IsNullOrEmpty(serializedCart))
        {
            return new(new ShoppingCart(), HasChanged: false, IsEmpty: true);
        }

        var cart = new ShoppingCart();
        var cartItems = JsonDocument.Parse(serializedCart).RootElement.GetProperty("Items").EnumerateArray();
        var hasChanged = false;

        // Update prices for all items in the shopping cart.
        foreach (var itemElement in cartItems)
        {
            var item = itemElement.ToObject<ShoppingCartItem>();
            cart.AddItem(item);

            if (item?.Prices.Any() != true) continue;

            var oldPrices = hasChanged ? null : item.GetPricesSimple();
            cart.SetPrices(item, item.Prices.Select(price => new PrioritizedPrice(
                price.Priority,
                _moneyService.EnsureCurrency(price.Price))));

            if (!hasChanged)
            {
                var newPrices = item.GetPricesSimple();
                hasChanged = oldPrices != newPrices;
            }
        }

        // Post-process attributes for concrete types according to field definitions (deserialization being essentially
        // non-polymorphic and without access to our type definition contextual information).
        // Also ensure, that there are no corrupted, missing or deleted products in the cart.
        var products = await GetProductsAsync(cart.Items.Select(item => item.ProductSku));
        var cartLineCount = cart.Items.Count;
        var newCartItems = await Task.WhenAll(cart.Items
            .Where(line =>
                line.Attributes != null &&
                !string.IsNullOrEmpty(line.ProductSku) &&
                products.ContainsKey(line.ProductSku))
            .Select(async line => new ShoppingCartItem(
                line.Quantity,
                line.ProductSku,
                await PostProcessAttributesAsync(line.Attributes, products[line.ProductSku]),
                line.Prices)));
        cart = cart.With(newCartItems);

        hasChanged = hasChanged || cart.Items.Count != cartLineCount;
        return new(cart, hasChanged, cart.Items.Count == 0);
    }

    public ISet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type) =>
        new HashSet<IProductAttributeValue>(
            line.Attributes is null
                ? []
                : line
                    .Attributes
                    .Where(attribute => attribute.Key?.Contains('.') == true)
                    .SelectWhere(attribute =>
                        type.GetFieldDefinition(attribute.Key) is ({ } partDefinition, { } fieldDefinition)
                            ? _attributeProviders
                                .Select(provider => provider.Parse(partDefinition, fieldDefinition, attribute.Value))
                                .FirstOrDefault(attributeValue => attributeValue != null)
                            : null));

    public async Task<ShoppingCartItem> ParseCartLineAsync(ShoppingCartLineUpdateModel line)
    {
        if (await _productService.GetProductAsync(line?.ProductSku) is not { } product) return null;

        var type = await GetTypeDefinitionAsync(product);
        return new ShoppingCartItem(line!.Quantity, line.ProductSku, ParseAttributes(line, type));
    }

    public async Task<ISet<IProductAttributeValue>> PostProcessAttributesAsync(
        IEnumerable<IProductAttributeValue> attributes,
        ProductPart productPart)
    {
        var type = await _contentDefinitionManager.GetTypeDefinitionAsync(productPart.ContentItem.ContentType);

        return attributes
            .CastWhere<BaseProductAttributeValue<object>>()
            .SelectWhere(attribute =>
                attribute.IsRaw() &&
                type.GetFieldDefinition(attribute.AttributeName) is ({ } partDefinition, { } fieldDefinition)
                    ? _attributeProviders
                        .SelectWhere(provider => provider.CreateFromValue(partDefinition, fieldDefinition, attribute.Value))
                        .FirstOrDefault()
                    : attribute)
            .ToHashSet();
    }

    private async Task<Dictionary<string, ProductPart>> GetProductsAsync(IEnumerable<string> skus) =>
        (await _productService.GetProductsAsync(skus)).ToDictionary(productPart => productPart.Sku);

    private async Task<Dictionary<string, ContentTypeDefinition>> ExtractTypeDefinitionsAsync(IEnumerable<ProductPart> products)
    {
        var typeDefinitions = await Task.WhenAll(products.Select(async productPart =>
        {
            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(productPart.ContentItem.ContentType);
            return new { Key = typeDefinition.Name, Value = typeDefinition };
        }));

        return typeDefinitions.GroupBy(td => td.Key)
                              .ToDictionary(group => group.Key, group => group.First().Value);
    }

    private Task<ContentTypeDefinition> GetTypeDefinitionAsync(ProductPart product) =>
        _contentDefinitionManager.GetTypeDefinitionAsync(product.ContentItem.ContentType);
}
