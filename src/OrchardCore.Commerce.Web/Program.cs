using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Logging;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLogHost();

var configuration = builder.Configuration;

builder.Services
    .AddSingleton(configuration)
    .AddOrchardCms(orchardCoreBuilder => orchardCoreBuilder
        // Enabling allowInlineStyle is necessary because style attributes are used in the Blog theme. Re-evaluate if
        // this is still true during the review of https://github.com/OrchardCMS/OrchardCore.Commerce/issues/300.
        .ConfigureSecurityDefaultsWithStaticFiles(new() { AllowInlineStyle = true })
        .EnableAutoSetupIfNotUITesting(configuration));

var app = builder.Build();
app.UseOrchardCore();
await app.RunAsync();

[SuppressMessage(
    "Design",
    "CA1050: Declare types in namespaces",
    Justification = "As described here: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0.")]
public partial class Program
{
    protected Program()
    {
        // Nothing to do here.
    }
}
