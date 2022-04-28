using System.Linq;

namespace InternationalAddress;

public static class ConcatenationHelper
{
    public static string JoinNotNullAndNotWhiteSpace(string separator, params string[] items) =>
        string.Join(separator, items.Where(string.IsNullOrWhiteSpace));
}
