using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Abstractions;

/// <summary>
/// A payment provider that can be used during checkout to add a charge to the order.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Gets the name used to identify additional payment provider data in the <see cref="ICheckoutViewModel"/> and as a
    /// suffix to the <c>Checkout{Name}</c> shape used to display the payment UI during checkout or in the order view.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Creates the additional data specific to the payment provider that's passed to the <c>Checkout{Name}</c> shape.
    /// </summary>
    /// <returns>
    /// Arbitrary data which will be set as the value in <see cref="IPaymentViewModel.PaymentProviderData"/>. If it
    /// returns <see langword="null"/> then the shape won't be displayed.
    /// </returns>
    Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model);

    /// <summary>
    /// Validates the data POSTed to the <see cref="PaymentController.Validate"/> action.
    /// </summary>
    Task ValidateAsync(IUpdateModelAccessor updateModelAccessor);

    /// <summary>
    /// Invoked at the end of <see cref="IPaymentService.FinalModificationOfOrderAsync"/>.
    /// </summary>
    Task FinalModificationOfOrderAsync(ContentItem order, string? shoppingCartId) => Task.CompletedTask;
}
