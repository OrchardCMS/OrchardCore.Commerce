namespace OrchardCore.Commerce.Payment.Exactly.Models;

/// <summary>
/// The data payload for responses using <see cref="ExactlyResponse{T}"/>.
/// </summary>
public interface IExactlyResponseData
{
    /// <summary>
    /// Gets the type name of the response.
    /// </summary>
    public string Type { get; }
}
