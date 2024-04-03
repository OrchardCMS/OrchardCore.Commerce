using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Exactly.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlyApiHandler : DelegatingHandler
{
    private readonly ExactlySettings _settings;

    public ExactlyApiHandler(IOptionsSnapshot<ExactlySettings> settings) => _settings = settings.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", "Api-Key " + _settings.ApiKey);

        // Has to be applied to every request. Even to GET requests, which is not permitted by .NET so it has to be
        // hacked in.
#pragma warning disable S3011
        const BindingFlags privateFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var allowedHeaderTypes = typeof(HttpHeaders).GetField("_allowedHeaderTypes", privateFieldFlags);
        allowedHeaderTypes!.SetValue(request.Headers, Enum.Parse(allowedHeaderTypes.FieldType, "All"));
        request.Headers.Add("Content-Type", "application/vnd.api+json");
#pragma warning restore S3011

        return base.SendAsync(request, cancellationToken);
    }
}
