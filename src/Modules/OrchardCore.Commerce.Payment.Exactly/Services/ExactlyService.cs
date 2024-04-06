using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrchardCore.Commerce.Abstractions.Exceptions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Exactly.Models;
using Refit;
using System;
using System.Threading;
using System.Threading.Tasks;
using FrontendException=Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

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
        return EvaluateResult(result);
    }

    public async Task<ChargeResponse> GetTransactionDetailsAsync(
        string transactionId,
        ChargeResponse.ChargeResponseStatus? waitForStatus = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transactionId);

        for (var i = 0; i < 100 && !cancellationToken.IsCancellationRequested; i++)
        {
            using var result = await _api.GetTransactionDetailsAsync(transactionId);
            var content = EvaluateResult(result);

            if (waitForStatus == null || content.Attributes.Status == waitForStatus) return content;

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException(
            $"Couldn't get the transaction \"{transactionId}\" with the status \"{waitForStatus}\" within " +
            $"the expected timeframe.");
    }

    private T EvaluateResult<T>(IApiResponse<ExactlyResponse<T>> result)
        where T : IExactlyResponseData
    {
        // If the request is not successful, try to parse the response error and throw a more specific FrontendException
        // instead of the ApiException.
        if (result.Error?.Content is { } error && error.StartsWith('{'))
        {
            try
            {
                var content = JsonConvert.DeserializeObject<ExactlyResponse<T>>(error);
                content.ThrowIfHasErrors();
            }
            catch (FrontendException)
            {
                throw;
            }
            catch
            {
                throw result.Error;
            }
        }

        // Handle any other non-specific ApiExceptions.
        if (result.Error is { } apiException) throw apiException;

        // In the unlikely case that the HTTP response is success but there was still an error somehow.
        result.Content!.ThrowIfHasErrors();

        return result.Content.Data;
    }
}
