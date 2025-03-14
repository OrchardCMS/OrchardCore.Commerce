﻿using OrchardCore.Commerce.Payment.Exactly.Models;
using Refit;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

/// <summary>
/// Wrapper for the Exactly API. See <see href="https://exactly.com/docs/api">here</see>. All method documentation is
/// copied from this source.
/// </summary>
public interface IExactlyApi
{
    /// <summary>
    /// The endpoint creates new transaction. Created transaction is processed in asynchronous manner, so there won't be
    /// any completed transaction in response to a request to this endpoint. Get transaction details endpoint or
    /// callback/webhook must be used to retrieve status of the transaction when it's completed.
    /// </summary>
    [Post("/api/v1/transactions")]
    Task<ApiResponse<ExactlyResponse<ChargeResponse>>> CreateTransactionAsync(
        [Body(buffered: true)] ExactlyDataWrapper<ExactlyRequest<ChargeRequest>> data);

    /// <summary>
    /// Returns details of a transaction.
    /// </summary>
    [Get("/api/v1/transactions/{transactionId}")]
    Task<ApiResponse<ExactlyResponse<ChargeResponse>>> GetTransactionDetailsAsync(string transactionId);
}
