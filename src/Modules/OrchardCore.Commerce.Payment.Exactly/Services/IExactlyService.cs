using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.ContentManagement;
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
    /// <param name="orderPart">
    /// The part whose <see cref="ContentItem.ContentItemId"/> and <see cref="OrderPart.LineItems"/>
    /// are used in the request.
    /// </param>
    /// <param name="total">
    /// The charge total to be sent in the request. If <see langword="null"/>, it's calculated using
    /// the total from the current shopping cart with all checkout events applied.
    /// </param>
    Task<ChargeResponse> CreateTransactionAsync(OrderPart orderPart, Amount? total = null);

    /// <summary>
    /// Returns details of a transaction.
    /// </summary>
    Task<ChargeResponse> GetTransactionDetailsAsync(
        string transactionId,
        ChargeResponse.ChargeResponseStatus? waitForStatus = null,
        CancellationToken cancellationToken = default);
}
