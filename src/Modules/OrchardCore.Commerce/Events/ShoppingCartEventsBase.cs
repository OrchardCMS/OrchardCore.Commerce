using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public abstract class ShoppingCartEventsBase : IShoppingCartEvents
{
    public virtual int Order => 0;

    public virtual Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        ShoppingCartDisplayingEventContext eventContext) =>
        Task.FromResult((eventContext.Headers, eventContext.Lines));

    public virtual Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item) =>
        Task.FromResult<LocalizedHtmlString>(null);
}
