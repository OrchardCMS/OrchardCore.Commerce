using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Exactly";

    public string Name => ProviderName;

    public ExactlyPaymentProvider()
    {
    }

    public Task<object> CreatePaymentProviderDataAsync(IPaymentViewModel model) =>
        throw new System.NotImplementedException();
    public Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(
        Controller controller,
        ContentItem order,
        string shoppingCartId) =>
        throw new System.NotImplementedException();
}
