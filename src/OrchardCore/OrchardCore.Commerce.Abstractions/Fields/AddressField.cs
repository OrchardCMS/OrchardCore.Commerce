using OrchardCore.Commerce.AddressDataType;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions.Fields;

/// <summary>
/// An abstraction of the data the address editor needs.
/// </summary>
public interface IAddressField
{
    /// <summary>
    /// Gets the address stored by this type.
    /// </summary>
    Address Address { get; }

    /// <summary>
    /// Gets a name of the field where <see cref="Address"/> should be saved in the user
    /// settings.
    /// </summary>
    string UserAddressToSave { get; }
}

public class AddressField : ContentField, IAddressField
{
    public Address Address { get; set; } = new();
    public string UserAddressToSave { get; set; }
}
