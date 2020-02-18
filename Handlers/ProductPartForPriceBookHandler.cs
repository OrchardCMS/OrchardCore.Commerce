using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers
{
    public class ProductPartForPriceBookHandler : ContentPartHandler<ProductPart>
    {
        // Using to prevent circular dependency injection
        private readonly IServiceProvider _serviceProvider;
        
        public ProductPartForPriceBookHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task RemovedAsync(RemoveContentContext context, ProductPart part)
        {
            var priceBookService = _serviceProvider.GetService<IPriceBookService>();
            await priceBookService.RemovePriceBookEntriesByProduct(part.ContentItem.ContentItemId);

            await base.RemovedAsync(context, part);
        }
    }
}
