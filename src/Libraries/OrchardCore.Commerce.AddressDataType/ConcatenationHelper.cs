using System.Linq;

namespace OrchardCore.Commerce.AddressDataType;

public static class ConcatenationHelper
{
    public static string JoinNotNullAndNotWhiteSpace(string separator, params string[] items) =>
        string.Join(separator, items.Where(item => !string.IsNullOrWhiteSpace(item)));
}
