using Microsoft.Extensions.Options;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class TestOptions<T> : IOptions<T> where T : class, new()
    {
        public TestOptions(T options) => Value = options;

        public T Value { get; }
    }
}
