namespace Newtonsoft.Json.Linq;

public static class JObjectExtensions
{
    public static T Get<T>(this JObject attribute, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (attribute.TryGetValue(key, out var token))
            {
                return token.ToObject<T>();
            }
        }

        return default;
    }
}
