using Microsoft.AspNetCore.Mvc.Localization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Abstractions.Exceptions;

[Serializable]
[SuppressMessage(
    "Maintainability",
    "S3925: ISerializable should be implemented correctly",
    Justification = "Exception(SerializationInfo info, StreamingContext context) is obsolete and should not be called.")]
public class FrontendException : Exception
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
