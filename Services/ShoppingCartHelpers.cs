using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services
{
    public class ShoppingCartHelpers : IShoppingCartHelpers
    {
        private readonly IEnumerable<IProductAttributeProvider> _attributeProviders;
        private readonly IProductService _productService;
        private readonly IMoneyService _moneyService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ShoppingCartHelpers(
            IEnumerable<IProductAttributeProvider> attributeProviders,
            IProductService productService,
            IMoneyService moneyService,
            IContentDefinitionManager contentDefinitionManager)
        {
            _attributeProviders = attributeProviders;
            _productService = productService;
            _moneyService = moneyService;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public ShoppingCartItem GetExistingItem(IList<ShoppingCartItem> cart, ShoppingCartItem item)
            => cart.FirstOrDefault(i => IsSameProductAs(i, item));

        public ShoppingCartLineViewModel GetExistingLine(ShoppingCartViewModel cart, ShoppingCartLineViewModel line)
            => cart.Lines.FirstOrDefault(i => IsSameProductAs(i, line));

        public int RemoveItem(IList<ShoppingCartItem> cart, ShoppingCartItem item)
        {
            var index = IndexOfProduct(cart, item);
            if (index != -1)
            {
                cart.RemoveAt(index);
            }
            return index;
        }

        public int IndexOfProduct(IList<ShoppingCartItem> cart, ShoppingCartItem item)
        {
            var index = 0;
            foreach (ShoppingCartItem line in cart)
            {
                if (IsSameProductAs(line, item)) return index;
                index++;
            }
            return -1;
        }

        public bool IsSameProductAs(ShoppingCartItem item, ShoppingCartItem other)
            => other.ProductSku == item.ProductSku && other.Attributes.SetEquals(item.Attributes);

        public bool IsSameProductAs(ShoppingCartLineViewModel line, ShoppingCartLineViewModel other)
            => other.ProductSku == line.ProductSku
                && (
                    ((line.Attributes is null || line.Attributes.Count == 0) && (other.Attributes is null || other.Attributes.Count == 0))
                    || (line.Attributes.Count == other.Attributes.Count && !line.Attributes.Except(other.Attributes).Any())
                );

        public async Task<IList<ShoppingCartItem>> ParseCart(ShoppingCartUpdateModel cart)
        {
            Dictionary<string, ProductPart> products = await GetProducts(cart.Lines.Select(l => l.ProductSku));
            Dictionary<string, ContentTypeDefinition> types = ExtractTypeDefinitions(products.Values);
            IList<ShoppingCartItem> parsedCart = cart.Lines
                .Select(l => new ShoppingCartItem(l.Quantity, l.ProductSku,
                    ParseAttributes(l, types[products[l.ProductSku].ContentItem.ContentType])
                 )).ToList();
            return parsedCart;
        }

        private Dictionary<string, ContentTypeDefinition> ExtractTypeDefinitions(IEnumerable<ProductPart> products)
            => products
                .Select(p => _contentDefinitionManager.GetTypeDefinition(p.ContentItem.ContentType))
                .Distinct()
                .ToDictionary(t => t.Name);

        private async Task<Dictionary<string, ProductPart>> GetProducts(IEnumerable<string> skus)
            => (await _productService.GetProducts(skus)).ToDictionary(p => p.Sku);

        public async Task<ShoppingCartItem> ParseCartLine(ShoppingCartLineUpdateModel line)
        {
            ProductPart product = await _productService.GetProduct(line.ProductSku);
            if (product is null) return null;
            ContentTypeDefinition type = GetTypeDefinition(product);
            var parsedLine = new ShoppingCartItem(line.Quantity, line.ProductSku, ParseAttributes(line, type));
            return parsedLine;
        }

        private ContentTypeDefinition GetTypeDefinition(ProductPart product)
            => _contentDefinitionManager.GetTypeDefinition(product.ContentItem.ContentType);

        public HashSet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type)
            => new HashSet<IProductAttributeValue>(
                line.Attributes is null ? Enumerable.Empty<IProductAttributeValue>() :
                line.Attributes
                .Select(attr =>
                {
                    (ContentTypePartDefinition attributePartDefinition, ContentPartFieldDefinition attributeFieldDefinition)
                        = GetFieldDefinition(type, attr.Key);
                    return _attributeProviders
                        .Select(provider => provider.Parse(attributePartDefinition, attributeFieldDefinition, attr.Value))
                        .FirstOrDefault(v => v != null);
                })
            );

        private static (ContentTypePartDefinition, ContentPartFieldDefinition) GetFieldDefinition(ContentTypeDefinition type, string attributeName)
        {
            string[] partAndField = attributeName.Split('.');
            return type
                .Parts.SelectMany(p => p.PartDefinition
                    .Fields
                    .Select(f => (p, f))
                    .Where(pf => p.Name == partAndField[0] && pf.f.Name == partAndField[1]))
                .First();
        }

        public async Task<IList<ShoppingCartItem>> Deserialize(string serializedCart)
        {
            if (String.IsNullOrEmpty(serializedCart))
            {
                return new List<ShoppingCartItem>();
            }
            var cart = JsonSerializer.Deserialize<List<ShoppingCartItem>>(serializedCart);
            // Actualize prices
            foreach (var item in cart)
            {
                if (item.Prices != null && item.Prices.Any())
                {
                    item.Prices = item.Prices
                        .Select(pp => new PrioritizedPrice(pp.Priority, _moneyService.EnsureCurrency(pp.Price)))
                        .ToList();
                }
            }
            // Post-process attributes for concrete types according to field definitions
            // (deserialization being essentially non-polymorphic and without access to our type definition
            // contextual information).
            Dictionary<string, ProductPart> products = await GetProducts(cart.Select(l => l.ProductSku));
            Dictionary<string, ContentTypeDefinition> types = ExtractTypeDefinitions(products.Values);
            var newCart = new List<ShoppingCartItem>(cart.Count);
            foreach (ShoppingCartItem line in cart)
            {
                if (line.Attributes is null) continue;
                var attributes = new HashSet<IProductAttributeValue>(line.Attributes.Count);
                foreach (RawProductAttributeValue attr in line.Attributes)
                {
                    ProductPart product = products[line.ProductSku];
                    ContentTypeDefinition type = types[product.ContentItem.ContentType];
                    (ContentTypePartDefinition attributePartDefinition, ContentPartFieldDefinition attributeFieldDefinition)
                        = GetFieldDefinition(type, attr.AttributeName);
                    IProductAttributeValue newAttr = _attributeProviders
                        .Select(provider => provider.CreateFromJsonElement(
                            attributePartDefinition,
                            attributeFieldDefinition,
                            attr.Value is null ? default(JsonElement) : (JsonElement)attr.Value))
                        .FirstOrDefault(v => v != null);
                    attributes.Add(newAttr);
                }
                newCart.Add(new ShoppingCartItem(line.Quantity, line.ProductSku, attributes, line.Prices));
            }
            return newCart;
        }

        public async Task<string> Serialize(IList<ShoppingCartItem> cart)
            => await Task.FromResult(JsonSerializer.Serialize(cart));
    }
}
