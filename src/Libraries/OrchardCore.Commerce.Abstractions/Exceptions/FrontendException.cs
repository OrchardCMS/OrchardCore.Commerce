using Microsoft.AspNetCore.Mvc.Localization;
using System;

namespace OrchardCore.Commerce.Abstractions.Exceptions;

[Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
// Exception(SerializationInfo info, StreamingContext context) is obsolete and should not be called.
public class FrontendException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
{
    public LocalizedHtmlString HtmlMessage { get; }

    public FrontendException(LocalizedHtmlString message)
        : base(message.Name) =>
        HtmlMessage = message;

    public FrontendException(string message)
        : base(message) =>
        HtmlMessage = new LocalizedHtmlString(message, message);

    public FrontendException()
    {
    }

    public FrontendException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
