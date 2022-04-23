using System.Linq;

namespace InternationalAddress;

public static class ConcatenationHelper
{
    public static string JoinWhenNotEmptyOrWhiteSpace(string separator, params string[] items) =>
        string.Join(separator, items.Where(item => string.IsNullOrWhiteSpace(item)));
}
