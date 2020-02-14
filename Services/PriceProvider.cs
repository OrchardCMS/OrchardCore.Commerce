using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A simple price provider that obtains a price from a product by looking for a `PricePart`
    /// </summary>
    public class PriceProvider : IPriceProvider
    {
        private readonly IProductService _productService;
        private readonly IMoneyService _moneyService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;

        public PriceProvider(
            IProductService productService,
            IMoneyService moneyService,
            IContentDefinitionManager contentDefinitionManager,
            ITypeActivatorFactory<ContentPart> contentPartFactory)
        {
            _productService = productService;
            _moneyService = moneyService;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartFactory = contentPartFactory;
        }

        public int Order => 0;

        public async Task AddPrices(IList<ShoppingCartItem> items)
        {
            var skus = items.Select(item => item.ProductSku).Distinct().ToArray();
            var skuProducts = (await _productService.GetProducts(skus))
                .ToDictionary(p => p.Sku);
            foreach (var item in items)
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var product))
                {
                    ContentItem contentItem = product.ContentItem;

                    // TODO: Swap the call below to extension method on ContentItem if approved in Orchard Core.
                    //       Also Remove the temporary implementation of PartsOfType and dependencies on ContentDefinitionManager
                    foreach (var pricePart in PartsOfType<PricePart>(contentItem))
                    {
                        if (pricePart.Price.Currency == _moneyService.CurrentDisplayCurrency)
                        {
                            item.Prices.Add(new PrioritizedPrice(0, pricePart.Price));
                        }
                    }

                    //foreach (var pricePart in contentItem.OfType<PricePart>()
                    //             .Where(p => p.Price.Currency == _moneyService.CurrentDisplayCurrency))
                    //{
                    //    item.Prices.Add(new PrioritizedPrice(0, pricePart.Price));
                    //}
                }
            }
        }

        /// <summary>
        /// Gets all content elements of a specific type.
        /// </summary>
        /// <typeparam name="TPart"></typeparam>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public IEnumerable<TPart> PartsOfType<TPart>(ContentItem contentItem) where TPart : ContentPart
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = contentItem.Get(partActivator.Type, partName) as TPart;

                if (part != null)
                {
                    yield return part;
                }
            }
        }
    }
}
