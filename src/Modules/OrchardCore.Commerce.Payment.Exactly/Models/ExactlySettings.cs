using OrchardCore.Commerce.Payment.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlySettings
{
    public ExactlyEnvironmentSettings Production { get; set; } = new();
    public ExactlyEnvironmentSettings Sandbox { get; set; } = new();

    /// <summary>
    /// Gets or sets the legacy base address. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the legacy project ID. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the legacy API key. Kept so existing site JSON can be deserialized.
    /// </summary>
    public string ApiKey { get; set; }

    public ExactlyEnvironmentSettings Get(PaymentEnvironment environment)
    {
        Production ??= new ExactlyEnvironmentSettings();
        Sandbox ??= new ExactlyEnvironmentSettings();
        MigrateLegacyKeys();

        return environment == PaymentEnvironment.Sandbox ? Sandbox : Production;
    }

    public void MigrateLegacyKeys()
    {
        Production ??= new ExactlyEnvironmentSettings();
        Sandbox ??= new ExactlyEnvironmentSettings();

        if (!string.IsNullOrEmpty(Production.ProjectId) ||
            !string.IsNullOrEmpty(Production.ApiKey) ||
            string.IsNullOrEmpty(ProjectId) && string.IsNullOrEmpty(ApiKey))
        {
            return;
        }

        Production.BaseAddress = string.IsNullOrWhiteSpace(BaseAddress)
            ? Production.BaseAddress
            : BaseAddress;
        Production.ProjectId = ProjectId;
        Production.ApiKey = ApiKey;
    }

    public void ClearLegacyKeys()
    {
        BaseAddress = null;
        ProjectId = null;
        ApiKey = null;
    }
}
