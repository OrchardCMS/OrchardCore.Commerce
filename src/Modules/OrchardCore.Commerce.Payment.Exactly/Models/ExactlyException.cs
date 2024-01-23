using System;
using System.Runtime.Serialization;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

[Serializable]
public class ExactlyException : Exception
{
    public ExactlyError Error { get; }

    public ExactlyException(ExactlyError error)
        : base($"{error.Code}: {error.Title} ({error.Details})") =>
        Error = error;

    public ExactlyException(string message)
        : base(message)
    {
    }

    public ExactlyException()
    {
    }

    public ExactlyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected ExactlyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
