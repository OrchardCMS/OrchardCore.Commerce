using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

public interface ICheckoutService
{
    Task<CheckoutViewModel> CreateCheckoutViewModelAsync(
        string shoppingCartId,
        Action<OrderPart> updateOrderPart = null);

    Task FinalModificationOfOrderAsync(ContentItem order);
}
