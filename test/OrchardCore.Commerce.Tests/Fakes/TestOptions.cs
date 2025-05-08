using Microsoft.Extensions.Options;

namespace OrchardCore.Commerce.Tests.Fakes;

internal sealed class TestOptions<T> : IOptions<T>
    where T : class, new()
{
    public T Value { get; }
    public TestOptions(T options) => Value = options;
}
