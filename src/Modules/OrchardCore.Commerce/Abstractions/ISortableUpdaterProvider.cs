using System.Collections.Generic;
using System.Linq;
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

public static class SortableUpdaterProviderExtensions
{
    /// <summary>
    /// Select the first provider where <see cref="ISortableUpdaterProvider{TModel}.IsApplicableAsync"/> evaluates to
    /// <see langword="true"/>, then calls <see cref="ISortableUpdaterProvider{TModel}.UpdateAsync"/> and returns the
    /// result. If none of the providers are applicable, it returns the provided <paramref name="model"/>.
    /// </summary>
    public static async Task<TModel> UpdateWithFirstProviderAsync<TModel>(
        this IEnumerable<ISortableUpdaterProvider<TModel>> providers,
        TModel model)
    {
        foreach (var provider in providers.OrderBy(provider => provider.Order))
        {
            if (await provider.IsApplicableAsync(model))
            {
                return await provider.UpdateAsync(model);
            }
        }

        return model;
    }
}
