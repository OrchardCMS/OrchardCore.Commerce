using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

    /// <summary>
    /// If the provided collection of <see cref="errors"/> is not empty, throws an exception with the included texts.
    /// </summary>
    /// <param name="errors">The possible collection of error texts.</param>
    public static void ThrowIfAny([AllowNull] ICollection<string> errors)
    {
        if (errors?.Any() != true) return;

        if (errors.Count == 1) throw new FrontendException(errors.Single());

        throw new FrontendException(new HtmlString("<br>").Join(
            errors.Select(error => new LocalizedHtmlString(error, error)).ToArray()));
    }

    /// <inheritdoc cref="ThrowIfAny(System.Collections.Generic.ICollection{string})"/>
    public static void ThrowIfAny([AllowNull] ICollection<LocalizedHtmlString> errors)
    {
        if (errors?.Any() != true) return;

        if (errors.Count == 1) throw new FrontendException(errors.Single());

        throw new FrontendException(new HtmlString("<br>").Join(errors.ToArray()));
    }
}
