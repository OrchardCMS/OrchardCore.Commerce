using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Payment.Exactly.Models;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

/// <summary>
/// A service for accessing the Exactly API.
/// </summary>
public interface IExactlyService
{
    /// <summary>
    /// Creates a new transaction for the current user based on the provided order.
    /// </summary>
    Task<ChargeResponse> CreateTransactionAsync(OrderPart orderPart);

    /// <summary>
    /// Returns details of a transaction.
    /// </summary>
    Task<ChargeResponse> GetTransactionDetailsAsync(
        string transactionId,
        ChargeResponse.ChargeResponseStatus? waitForStatus = null,
        CancellationToken cancellationToken = default);
}
