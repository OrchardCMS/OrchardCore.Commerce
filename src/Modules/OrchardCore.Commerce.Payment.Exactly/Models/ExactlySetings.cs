using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlySettings
{
    [Required]
    public string BaseAddress { get; set; } = "https://api.exactly.com/";

    [Required]
    public string ProjectId { get; set; }

    public string ApiKey { get; set; }

    public void CopyTo(ExactlySettings target, bool copyPassword = true)
    {
        if (Uri.TryCreate(BaseAddress, UriKind.Absolute, out var _)) target.BaseAddress = BaseAddress;
        if (!string.IsNullOrWhiteSpace(ProjectId)) target.ProjectId = ProjectId;
        if (copyPassword && !string.IsNullOrWhiteSpace(ApiKey)) target.ApiKey = ApiKey;
    }
}
