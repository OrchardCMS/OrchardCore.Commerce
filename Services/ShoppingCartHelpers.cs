using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services
{
    public class ShoppingCartHelpers : IShoppingCartHelpers
    {
        private readonly IList<IProductAttributeParseService> _parseServices;
        private readonly IProductService _productService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ShoppingCartHelpers(
            IList<IProductAttributeParseService> parseServices,
            IProductService productService,
            IContentDefinitionManager contentDefinitionManager)
        {
            _parseServices = parseServices;
            _productService = productService;
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
            foreach (var i in cart)
            {
                if (IsSameProductAs(i, item)) return index;
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
            Dictionary<string, ProductPart> products =
                (await _productService.GetProducts(cart.Lines.Select(l => l.ProductSku)))
                .ToDictionary(p => p.Sku);
            Dictionary<string, ContentTypeDefinition> types = products.Values
                .Select(p => _contentDefinitionManager.GetTypeDefinition(p.ContentItem.ContentType))
                .Distinct()
                .ToDictionary(t => t.Name);
            IList<ShoppingCartItem> parsedCart = cart.Lines
                .Select(l => new ShoppingCartItem(l.Quantity, l.ProductSku,
                    ParseAttributes(l, types[products[l.ProductSku].ContentItem.ContentType])
                 )).ToList();
            return parsedCart;
        }

        public async Task<ShoppingCartItem> ParseCartLine(ShoppingCartLineUpdateModel line)
        {
            ProductPart product = await _productService.GetProduct(line.ProductSku);
            ContentTypeDefinition type = _contentDefinitionManager.GetTypeDefinition(product.ContentItem.ContentType);
            var parsedLine = new ShoppingCartItem(line.Quantity, line.ProductSku, ParseAttributes(line, type));
            return parsedLine;
        }

        public HashSet<IProductAttributeValue> ParseAttributes(ShoppingCartLineUpdateModel line, ContentTypeDefinition type)
            => new HashSet<IProductAttributeValue>(
                line.Attributes is null ? Enumerable.Empty<IProductAttributeValue>() :
                line.Attributes
                .Select(a =>
                {
                    ContentPartFieldDefinition attributeFieldDefinition = type
                        .Parts.SelectMany(p => p.PartDefinition.Fields)
                        .First(f => f.Name == a.Key);
                    return _parseServices
                        .Select(s => s.Parse(attributeFieldDefinition, a.Value))
                        .FirstOrDefault(v => v != null);
                })
            );
    }
}
