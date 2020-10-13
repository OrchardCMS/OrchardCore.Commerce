using System.Text.Json;

namespace OrchardCore.Commerce.Serialization
{
    public static class JsonElementSerializer
    {
        /// <summary>
        /// Transforms a JSON element into the required type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="jsonElement">The element.</param>
        /// <returns>The deserialized instance of T.</returns>
        /// <remarks>This will no longer be necessary when the BCL supports it natively.</remarks>
        public static T ToObject<T>(this JsonElement jsonElement)
        {
            string elementText = jsonElement.GetRawText();
            return JsonSerializer.Deserialize<T>(elementText);
        }
    }
}