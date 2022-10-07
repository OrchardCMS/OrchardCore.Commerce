using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Serialization;
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
        var types = ExtractTypeDefinitions(products.Values);

        return new ShoppingCart(cart.Lines
            .Where(updateModel => updateModel.Quantity > 0)
            .Select(updateModel => new ShoppingCartItem(
            updateModel.Quantity,
            updateModel.ProductSku,
            ParseAttributes(updateModel, types[products[updateModel.ProductSku].ContentItem.ContentType]))));
    }

    public async Task<ShoppingCart> DeserializeAsync(string serializedCart)
    {
        var cart = new ShoppingCart();
        if (string.IsNullOrEmpty(serializedCart))
        {
            return cart;
        }

        var cartItems = JsonDocument.Parse(serializedCart).RootElement.GetProperty("Items").EnumerateArray();

        // Update prices for all items in the shopping cart.
        foreach (var itemElement in cartItems)
        {
            var item = itemElement.ToObject<ShoppingCartItem>();
            cart.AddItem(item);

            if (item.Prices != null && item.Prices.Any())
            {
                cart.SetPrices(
                item,
                item.Prices.Select(pp => new PrioritizedPrice(pp.Priority, _moneyService.EnsureCurrency(pp.Price))));
            }
        }

        // Post-process attributes for concrete types according to field definitions (deserialization being essentially
        // non-polymorphic and without access to our type definition contextual information).
        var products = await GetProductsAsync(cart.Items.Select(item => item.ProductSku));
        var newCartItems = PostProcessAttributes(cart, products);

        return cart.With(newCartItems);
    }

    public ISet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type) =>
        new HashSet<IProductAttributeValue>(
        line.Attributes is null
            ? Enumerable.Empty<IProductAttributeValue>()
            : line.Attributes
                .Select(attr =>
                {
                    var (attributePartDefinition, attributeFieldDefinition) = GetFieldDefinition(type, attr.Key);
                    return _attributeProviders
                        .Select(provider => provider.Parse(attributePartDefinition, attributeFieldDefinition, attr.Value))
                        .FirstOrDefault(attributeValue => attributeValue != null);
                }));

    public async Task<ShoppingCartItem> ParseCartLineAsync(ShoppingCartLineUpdateModel line)
    {
        var product = await _productService.GetProductAsync(line.ProductSku);
        if (product is null) return null;
        var type = GetTypeDefinition(product);
        var parsedLine = new ShoppingCartItem(line.Quantity, line.ProductSku, ParseAttributes(line, type));
        return parsedLine;
    }

    private async Task<Dictionary<string, ProductPart>> GetProductsAsync(IEnumerable<string> skus) =>
        (await _productService.GetProductsAsync(skus)).ToDictionary(productPart => productPart.Sku);

    private List<ShoppingCartItem> PostProcessAttributes(ShoppingCart cart, Dictionary<string, ProductPart> products)
    {
        var types = ExtractTypeDefinitions(products.Values);
        var newCartItems = new List<ShoppingCartItem>(cart.Count);

        foreach (var line in cart.Items)
        {
            if (line.Attributes is null) continue;

            var attributes = new HashSet<IProductAttributeValue>(line.Attributes.Count);

            foreach (var attr in line.Attributes.OfType<RawProductAttributeValue>())
            {
                var product = products[line.ProductSku];
                var type = types[product.ContentItem.ContentType];
                var (attributePartDefinition, attributeFieldDefinition) = GetFieldDefinition(type, attr.AttributeName);
                if (attributePartDefinition != null && attributeFieldDefinition != null)
                {
                    var newAttr = _attributeProviders
                        .Select(provider => provider.CreateFromJsonElement(
                        attributePartDefinition,
                        attributeFieldDefinition,
                        attr.Value is JsonElement element ? element : default))
                        .FirstOrDefault(attributeValue => attributeValue != null);
                    attributes.Add(newAttr);
                }
                else
                {
                    attributes.Add(attr);
                }
            }

            newCartItems.Add(new ShoppingCartItem(line.Quantity, line.ProductSku, attributes, line.Prices));
        }

        return newCartItems;
    }

    private Dictionary<string, ContentTypeDefinition> ExtractTypeDefinitions(IEnumerable<ProductPart> products) =>
        products
            .Select(productPart => _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType))
            .GroupBy(typeDefinition => typeDefinition.Name)
            .ToDictionary(group => group.Key, group => group.First());

    private static (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(ContentTypeDefinition type, string attributeName)
    {
        var partAndField = attributeName.Split('.');
        var partName = partAndField[0];
        var fieldName = partAndField[1];

        return type
            .Parts
            .Where(partDefinition => partDefinition.Name == partName)
            .SelectMany(partDefinition => partDefinition
                .PartDefinition
                .Fields
                .Select(fieldDefinition => (PartDefinition: partDefinition, FieldDefinition: fieldDefinition))
                .Where(pair => pair.FieldDefinition.Name == fieldName))
            .FirstOrDefault();
    }

    private ContentTypeDefinition GetTypeDefinition(ProductPart product) =>
        _contentDefinitionManager.GetTypeDefinition(product.ContentItem.ContentType);
}
