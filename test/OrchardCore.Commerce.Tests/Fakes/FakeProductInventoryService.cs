using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeProductInventoryService : IProductInventoryService
{
    // IProductInventoryService's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public Task<IList<ShoppingCartItem>> UpdateInventoriesAsync(IList<ShoppingCartItem> items) =>
        throw new NotSupportedException();

    // IProductInventoryService's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public Task<bool> VerifyLinesAsync(IList<ShoppingCartLineViewModel> lines) =>
        throw new NotSupportedException();
}
