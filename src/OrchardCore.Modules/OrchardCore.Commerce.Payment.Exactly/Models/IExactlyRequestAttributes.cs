namespace OrchardCore.Commerce.Payment.Exactly.Models;

/// <summary>
/// The data payload for requests using <see cref="ExactlyRequest{T}"/>.
/// </summary>
public interface IExactlyRequestAttributes
{
    /// <summary>
    /// Gets the type name of the request. This is used by <see cref="ExactlyRequest{T}.Type"/>.
    /// </summary>
    public string Type { get; }
}
