using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A base for provider interfaces such as <see cref="IPriceProvider"/> and <see cref="ITaxProvider"/>.
/// </summary>
/// <typeparam name="TModel">The object to be updated.</typeparam>
public interface ISortableUpdaterProvider<TModel>
{
    /// <summary>
    /// Gets the value used to sort providers in ascending order. The first one that's applicable (<see
    /// cref="IsApplicableAsync"/> returns <see langword="true"/>) will be used.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns an update version of <paramref name="model"/>. This should not mutate the input directly, but deep
    /// immutability (e.g. of the items if <paramref name="model"/> is a collection) is not guaranteed.
    /// </summary>
    Task<TModel> UpdateAsync(TModel model);

    /// <summary>
    /// Checks whether or not the provider is applicable for the <see cref="model"/>.
    /// </summary>
    Task<bool> IsApplicableAsync(TModel model);
}
