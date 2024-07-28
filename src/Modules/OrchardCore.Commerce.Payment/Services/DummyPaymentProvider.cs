using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Services;

public class DummyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Dummy";

    private readonly IClock _clock;
    private readonly IHttpContextAccessor _hca;
    private readonly Lazy<IPaymentService> _paymentServiceLazy;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;

    public string Name => ProviderName;

    public DummyPaymentProvider(
        IClock clock,
        IHttpContextAccessor hca,
        Lazy<IPaymentService> paymentServiceLazy,
        IShoppingCartHelpers shoppingCartHelpers)
    {
        _clock = clock;
        _hca = hca;
        _paymentServiceLazy = paymentServiceLazy;
        _shoppingCartHelpers = shoppingCartHelpers;
    }

    public Task<object?> CreatePaymentProviderDataAsync(IPaymentViewModel model, bool isPaymentRequest = false) =>
        // This provider doesn't have any special data, and it should only be displayed during development even if the
        // feature is enabled. So if the condition is met a blank object is returned, otherwise null which will cause
        // the provider to be skipped when used through the viewModel.WithProviderDataAsync(providers) method.
        Task.FromResult(_hca.HttpContext.IsDevelopmentAndLocalhost() ? new object() : null);

    public async Task<IActionResult> UpdateAndRedirectToFinishedOrderAsync(Controller controller, ContentItem order, string? shoppingCartId)
    {
        var createdUtc = order.CreatedUtc ?? _clock.UtcNow;
        var cart = await _shoppingCartHelpers.CreateShoppingCartViewModelAsync(shoppingCartId, order);
        var totals = cart
            .GetTotalsOrThrowIfEmpty()
            .Select((total, index) => new Commerce.Abstractions.Models.Payment(
                Kind: "Dummy Payment",
                TransactionId: $"{order.ContentItemId}:{index.ToTechnicalString()}",
                ChargeText: $"Dummy transaction of {total.Currency.EnglishName}.",
                total,
                createdUtc));

        return await _paymentServiceLazy
            .Value
            .UpdateAndRedirectToFinishedOrderAsync(
                controller,
                order,
                shoppingCartId,
                ProviderName,
                _ => totals);
    }
}
