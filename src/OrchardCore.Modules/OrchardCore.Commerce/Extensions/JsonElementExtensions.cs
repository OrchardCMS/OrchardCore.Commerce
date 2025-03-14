namespace System.Text.Json;

public static class JsonElementExtensions
{
    /// <summary>
    /// Transforms a JSON element into the required type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="jsonElement">The element.</param>
    /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// <para>This will no longer be necessary when the BCL supports it natively.</para>
    /// </remarks>
    public static T ToObject<T>(this JsonElement jsonElement)
    {
        var elementText = jsonElement.GetRawText();
        return JsonSerializer.Deserialize<T>(elementText);
    }
}
