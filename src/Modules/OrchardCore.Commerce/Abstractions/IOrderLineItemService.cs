using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service for order line items.
/// </summary>
public interface IOrderLineItemService
{
    /// <summary>
    /// Returns the list of <see cref="OrderLineItemViewModel"/>s and the total <see cref="Amount"/> from the provided
    /// list of <paramref name="lineItems"/>.
    /// </summary>
    Task<(IList<OrderLineItemViewModel> ViewModels, Amount Total)> CreateOrderLineItemViewModelsAndTotalAsync(
        IList<OrderLineItem> lineItems,
        OrderPart orderPart);
}
