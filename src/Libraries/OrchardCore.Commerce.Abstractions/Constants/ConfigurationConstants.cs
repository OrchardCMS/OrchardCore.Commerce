namespace OrchardCore.Commerce.Abstractions.Constants;

public static class ConfigurationConstants
{
    public const string Commerce = nameof(Commerce);
    public const string HeadersDisplayNames = nameof(HeadersDisplayNames);
    public const string HeadersConfig = $"{Commerce}:{HeadersDisplayNames}";
    public const string OldHeadersConfig = "OrchardCoreCommerce_HeadersDisplayNames";
}
