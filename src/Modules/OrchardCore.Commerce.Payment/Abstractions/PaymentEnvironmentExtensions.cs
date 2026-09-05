using System;

namespace OrchardCore.Commerce.Payment.Abstractions;

public static class PaymentEnvironmentExtensions
{
    public const string QueryParameterName = "environment";
    public const string SandboxSuffix = "Sandbox";

    /// <summary>
    /// Returns <paramref name="productionName"/> for production, or <paramref name="productionName"/> +
    /// <see cref="SandboxSuffix"/> for sandbox.
    /// </summary>
    public static string ForEnvironment(this string productionName, PaymentEnvironment environment) =>
        environment == PaymentEnvironment.Sandbox
            ? productionName + SandboxSuffix
            : productionName;

    /// <summary>
    /// Parses an environment query/form value. Missing, empty, or unknown values mean
    /// <see cref="PaymentEnvironment.Production"/>.
    /// </summary>
    public static PaymentEnvironment Parse(string? value) =>
        string.Equals(value, nameof(PaymentEnvironment.Sandbox), StringComparison.OrdinalIgnoreCase)
            ? PaymentEnvironment.Sandbox
            : PaymentEnvironment.Production;

    /// <summary>
    /// Maps a payment provider name to an environment using the production name plus <see cref="SandboxSuffix"/>.
    /// </summary>
    public static PaymentEnvironment FromProviderName(string? providerName, string productionName) =>
        string.Equals(
            providerName,
            productionName.ForEnvironment(PaymentEnvironment.Sandbox),
            StringComparison.OrdinalIgnoreCase)
            ? PaymentEnvironment.Sandbox
            : PaymentEnvironment.Production;

    /// <summary>
    /// Returns the lowercase query value for <paramref name="environment"/>, or <see langword="null"/> for production
    /// so existing URLs stay unchanged.
    /// </summary>
    public static string? ToQueryValue(this PaymentEnvironment environment) =>
        environment == PaymentEnvironment.Sandbox
            ? nameof(PaymentEnvironment.Sandbox).ToLowerInvariant()
            : null;
}
