using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlySettings
{
    [Required]
    public string BaseAddress { get; set; }

    public string ApiKey { get; set; }

    public string ProjectId { get; set; }

    public void CopyTo(ExactlySettings target)
    {
        if (Uri.TryCreate(BaseAddress, UriKind.Absolute, out var _)) target.BaseAddress = BaseAddress;
        if (!string.IsNullOrWhiteSpace(ApiKey)) target.ApiKey = ApiKey;
        if (!string.IsNullOrWhiteSpace(ProjectId)) target.ProjectId = ProjectId;
    }
}
