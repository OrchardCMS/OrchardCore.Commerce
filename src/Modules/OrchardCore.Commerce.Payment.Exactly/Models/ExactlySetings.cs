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

    public void CopyTo(ExactlySettings target)
    {
        if (Uri.TryCreate(BaseAddress, UriKind.Absolute, out var baseUri)) target.BaseAddress = baseUri.AbsoluteUri;
        if (!string.IsNullOrWhiteSpace(ProjectId)) target.ProjectId = ProjectId.Trim();
        if (!string.IsNullOrWhiteSpace(ApiKey)) target.ApiKey = ApiKey.Trim();
    }
}
