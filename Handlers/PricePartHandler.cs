using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers
{
    public class PricePartHandler : ContentPartHandler<PricePart>
    {
        public override Task InitializingAsync(InitializingContentContext context, PricePart part)
        {
            return Task.CompletedTask;
        }
    }
}