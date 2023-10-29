using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Extension points for events related to orders.
/// </summary>
public interface IOrderEvents
{
    /// <summary>
    /// Invoked when the <paramref name="order"/> is set to the <c>Ordered</c> state.
    /// </summary>
    Task OrderedAsync(ContentItem order, string shoppingCartId);
}
