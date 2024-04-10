using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.AddressDataType.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.AddressDataType.Services;

/// <summary>
/// An <see cref="Address"/> updater that concatenates the <see cref="CommonNameParts"/> in the "western" name order.
/// </summary>
/// <remarks><para>If you haven't yet, please read the article <a
/// href="https://www.kalzumeus.com/2010/06/17/falsehoods-programmers-believe-about-names/">Falsehoods Programmers
/// Believe About Names</a> and consider if the limitations in this updater will reasonably serve your audience or if
/// you should make a custom one.</para></remarks>
public class WesternCommonNamePartsAddressUpdater : IAddressUpdater
{
    public Task UpdateAsync(Address address)
    {
        if (!address.NameParts.Any()) return Task.CompletedTask;

        var parts = new List<string>(capacity: 4);
        AddNamePart(parts, address, CommonNameParts.Honorific);
        AddNamePart(parts, address, CommonNameParts.GivenName);
        AddNamePart(parts, address, CommonNameParts.MiddleName);
        AddNamePart(parts, address, CommonNameParts.FamilyName);

        if (parts.Count != 0)
        {
            address.Name = string.Join(' ', parts);
        }

        return Task.CompletedTask;
    }

    private static void AddNamePart(List<string> parts, Address address, string key)
    {
        if (address.NameParts.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            parts.Add(value);
        }
    }
}
