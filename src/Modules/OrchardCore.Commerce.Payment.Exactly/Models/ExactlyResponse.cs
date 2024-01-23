using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlyResponse<T>
    where T : IExactlyResponseData
{
    public IExactlyResponseData Data { get; set; }
    public IList<ExactlyError> Errors { get; set; }

    public void ThrowIfHasErrors()
    {
        if (Errors?.Any() != true) return;

        if (Errors.Count == 1) throw new ExactlyException(Errors.Single());

        throw new AggregateException(Errors.Select(error => new ExactlyException(error)));
    }
}
