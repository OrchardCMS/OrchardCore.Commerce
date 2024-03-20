using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Exactly.Models;
using Refit;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlyService : IExactlyService
{
    private readonly IExactlyApi _api;
    private readonly IHttpContextAccessor _hca;

    public ExactlyService(IExactlyApi api, IHttpContextAccessor hca)
    {
        _api = api;
        _hca = hca;
    }

    public async Task<ChargeResponse> CreateTransactionAsync(OrderPart orderPart)
    {
        var charge = await ChargeRequest.CreateUserAsync(orderPart, _hca.HttpContext);
        var request = new ExactlyDataWrapper<ExactlyRequest<ChargeRequest>>(
            new ExactlyRequest<ChargeRequest> { Attributes = charge });

        using var result = await _api.CreateTransactionAsync(request);
        return await EvaluateResultAsync(result);
    }

    public async Task<ChargeResponse> GetTransactionDetailsAsync(string transactionId)
    {
        using var result = await _api.GetTransactionDetailsAsync(transactionId);
        return await EvaluateResultAsync(result);
    }

    private async Task<ChargeResponse> EvaluateResultAsync(ApiResponse<ExactlyResponse<ChargeResponse>> result)
    {
        await result.EnsureSuccessStatusCodeAsync();
        result.Content!.ThrowIfHasErrors();

        return result.Content.Data;
    }
}
