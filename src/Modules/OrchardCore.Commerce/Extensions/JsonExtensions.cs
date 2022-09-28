using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Linq;

public static class JsonExtensions
{
    [SuppressMessage(
        "Major Code Smell",
        "S1168:Empty arrays and collections should be returned instead of null",
        Justification = "An empty JObject makes no sense here.")]
    public static JObject GetJObject(this JObject node, params string[] properties)
    {
        foreach (var property in properties)
        {
            if (!node.TryGetValue(property, out var child) || child is not JObject childObject) return null;
            node = childObject;
        }

        return node;
    }
}
