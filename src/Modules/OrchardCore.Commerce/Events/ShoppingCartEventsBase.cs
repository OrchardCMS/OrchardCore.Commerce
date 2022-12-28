using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public abstract class ShoppingCartEventsBase : IShoppingCartEvents
{
    public virtual int Order => 0;

    public virtual Task<(IList<Amount> Totals, IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        IList<Amount> totals, IList<LocalizedHtmlString> headers, IList<ShoppingCartLineViewModel> lines) =>
            Task.FromResult((totals, headers, lines));

    public virtual Task<bool> VerifyingItemAsync(ShoppingCartItem item) => Task.FromResult(true);
}
