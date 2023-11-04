using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class DummyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Dummy";

    private readonly IHttpContextAccessor _hca;

    public string Name => ProviderName;

    public DummyPaymentProvider(IHttpContextAccessor hca) =>
        _hca = hca;

    public Task<object?> CreatePaymentProviderDataAsync(IPaymentViewModel model) =>
        // This provider doesn't have any special data, and it should only be displayed during development even if the
        // feature is enabled. So if the condition is met a blank object is returned, otherwise null which will cause
        // the provider to be skipped when used through the viewModel.WithProviderDataAsync(providers) method.
        Task.FromResult(_hca.HttpContext.IsDevelopmentAndLocalhost() ? new object() : null);

    public Task ValidateAsync(IUpdateModelAccessor updateModelAccessor) => Task.CompletedTask;
}
