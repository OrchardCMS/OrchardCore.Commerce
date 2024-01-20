using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Exactly.Models;
using System.Net.Http;
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

        return base.SendAsync(request, cancellationToken);
    }
}
