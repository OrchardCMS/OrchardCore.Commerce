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

            if (item?.Prices.Any() == true)
            {
                cart.SetPrices(
                    item,
                    item.Prices.Select(price => new PrioritizedPrice(price.Priority, _moneyService.EnsureCurrency(price.Price))));
            }
        }

        // Post-process attributes for concrete types according to field definitions (deserialization being essentially
        // non-polymorphic and without access to our type definition contextual information).
        var products = await GetProductsAsync(cart.Items.Select(item => item.ProductSku));
        var newCartItems = cart
            .Items
            .Where(line => line.Attributes != null)
            .Select(line => new ShoppingCartItem(
                line.Quantity,
                line.ProductSku,
                PostProcessAttributes(line.Attributes, products[line.ProductSku]),
                line.Prices))
            .ToList();

        return cart.With(newCartItems);
    }

    public ISet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type) =>
        new HashSet<IProductAttributeValue>(
        line.Attributes is null
            ? Enumerable.Empty<IProductAttributeValue>()
            : line
                .Attributes
                .Where(attribute => attribute.Key.Contains('.'))
                .Select(attribute =>
                {
                    var (attributePartDefinition, attributeFieldDefinition) = type.GetFieldDefinition(attribute.Key);
                    return _attributeProviders
                        .Select(provider => provider.Parse(attributePartDefinition, attributeFieldDefinition, attribute.Value))
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

    public ISet<IProductAttributeValue> PostProcessAttributes(IEnumerable<IProductAttributeValue> attributes, ProductPart productPart)
    {
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);

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

    private Dictionary<string, ContentTypeDefinition> ExtractTypeDefinitions(IEnumerable<ProductPart> products) =>
        products
            .Select(productPart => _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType))
            .GroupBy(typeDefinition => typeDefinition.Name)
            .ToDictionary(group => group.Key, group => group.First());

    private ContentTypeDefinition GetTypeDefinition(ProductPart product) =>
        _contentDefinitionManager.GetTypeDefinition(product.ContentItem.ContentType);
}
