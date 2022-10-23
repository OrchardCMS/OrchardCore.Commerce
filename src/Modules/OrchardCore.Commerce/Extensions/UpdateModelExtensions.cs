using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Extensions;

public static class UpdateModelExtensions
{
    public static IEnumerable<ModelError> GetModelErrors(this IUpdateModel updateModel) =>
        updateModel
            .ModelState
            .Values
            .SelectMany(entry => entry.Errors)
            .Where(error => !string.IsNullOrWhiteSpace(error.ErrorMessage));
}
