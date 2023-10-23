using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service for accessing and managing this scope's Stripe <see cref="RequestOptions"/> value.
/// </summary>
public interface IRequestOptionsService
{
    /// <summary>
    /// Returns this scope's request options. If it doesn't exist yet, creates one by referring to the site settings.
    /// </summary>
    Task<RequestOptions> GetOrCreateRequestOptionsAsync();

    /// <summary>
    /// Sets the <see cref="RequestOptions.IdempotencyKey"/> to a new unique value. If the <see cref="RequestOptions"/>
    /// is not yet initialized, it uses <see cref="GetOrCreateRequestOptionsAsync"/> to create it first.
    /// </summary>
    Task<RequestOptions> SetIdempotencyKeyAsync();
}
