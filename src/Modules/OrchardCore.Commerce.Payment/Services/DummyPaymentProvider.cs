using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.ViewModels;
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
    private readonly IHtmlLocalizer<DummyPaymentProvider> H;
    public string Name => ProviderName;

    public DummyPaymentProvider(
        IClock clock,
        IHttpContextAccessor hca,
        Lazy<IPaymentService> paymentServiceLazy,
        IShoppingCartHelpers shoppingCartHelpers,
        IHtmlLocalizer<DummyPaymentProvider> htmlLocalizer)
    {
        _clock = clock;
        _hca = hca;
        _paymentServiceLazy = paymentServiceLazy;
        _shoppingCartHelpers = shoppingCartHelpers;
        H = htmlLocalizer;
    }

    public Task<object?> CreatePaymentProviderDataAsync(IPaymentViewModel model, bool isPaymentRequest = false, string? shoppingCartId = null) =>
        // This provider doesn't have any special data, and it should only be displayed during development even if the
        // feature is enabled. So if the condition is met a blank object is returned, otherwise null which will cause
        // the provider to be skipped when used through the viewModel.WithProviderDataAsync(providers) method.
        Task.FromResult(_hca.HttpContext.IsDevelopmentAndLocalhost() ? new object() : null);

    public async Task<PaymentOperationStatusViewModel> UpdateAndRedirectToFinishedOrderAsync(
        ContentItem order,
        string? shoppingCartId
        )
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

        try
        {
            return await _paymentServiceLazy
            .Value
            .UpdateAndRedirectToFinishedOrderAsync(
                order,
                shoppingCartId,
                ProviderName,
                _ => totals);
        }
        catch (Exception ex)
        {
            return new PaymentOperationStatusViewModel
            {
                Status = PaymentOperationStatus.Failed,
                ShowMessage = H["You have paid the bill, but this system did not record it. Please contact the administrators."],
                HideMessage = ex.Message,
                Content = order,
            };
        }
    }
}
