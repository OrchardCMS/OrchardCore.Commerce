using System.Linq;

namespace InternationalAddress;

public static class ConcatenationHelper
{
    public static string JoinNonNullOrWhiteSpace(string separator, params string[] items) =>
        string.Join(separator, items.Where(item => string.IsNullOrWhiteSpace(item)));
}
