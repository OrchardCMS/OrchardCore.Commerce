using System.Threading.Tasks;

namespace OrchardCore.Commerce.AddressDataType.Abstractions;

/// <summary>
/// A service for updating <see cref="Address"/>, such as to fill the <see cref="Address.Name"/> property from the
/// values in <see cref="Address.NameParts"/>.
/// </summary>
public interface IAddressUpdater
{
    /// <summary>
    /// Updates the provided <paramref name="address"/>.
    /// </summary>
    public Task UpdateAsync(Address address);
}
