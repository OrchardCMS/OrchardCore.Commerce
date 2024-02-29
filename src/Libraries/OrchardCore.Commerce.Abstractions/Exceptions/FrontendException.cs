using Microsoft.AspNetCore.Mvc.Localization;
using System;

namespace OrchardCore.Commerce.Abstractions.Exceptions;

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
