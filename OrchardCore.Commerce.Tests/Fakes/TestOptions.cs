using Microsoft.Extensions.Options;

namespace OrchardCore.Commerce.Tests.Fakes;

public class TestOptions<T> : IOptions<T>
    where T : class, new()
{
    public T Value { get; }
    public TestOptions(T options) => Value = options;
}
