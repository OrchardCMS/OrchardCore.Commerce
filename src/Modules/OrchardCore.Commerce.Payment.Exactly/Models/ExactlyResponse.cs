using OrchardCore.Commerce.Abstractions.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlyResponse<T>
    where T : IExactlyResponseData
{
    public T Data { get; set; }
    public IList<ExactlyError> Errors { get; set; }

    public void ThrowIfHasErrors()
    {
        if (Errors?.Any() != true) return;

        var errors = Errors.Select(error => $"{error.Code}: {error.Title} ({error.Details})").ToList();
        FrontendException.ThrowIfAny(errors);
    }
}
