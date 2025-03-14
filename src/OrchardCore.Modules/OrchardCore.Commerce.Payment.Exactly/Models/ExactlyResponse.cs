using System.Collections.Generic;
using System.Linq;
using FrontendException = Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlyResponse<T>
    where T : IExactlyResponseData
{
    public T Data { get; set; }
    public IEnumerable<ExactlyError> Errors { get; set; }

    public void ThrowIfHasErrors()
    {
        var errors = Errors?
            .Select(error => $"{error.Code}: {error.Title} ({error.Details?.Trim()})".Replace(" ()", string.Empty))
            .Distinct()
            .ToList();

        FrontendException.ThrowIfAny(errors);
    }
}
