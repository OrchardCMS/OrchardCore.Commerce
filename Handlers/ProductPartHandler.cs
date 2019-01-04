using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers
{
    public class ProductPartHandler : ContentPartHandler<ProductPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, ProductPart part)
        {
            // TODO: part initialization goes here.

            return Task.CompletedTask;
        }
    }
}