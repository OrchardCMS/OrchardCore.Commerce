using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Descriptors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class AddressUpdaterShapeTableProvider : IShapeTableProvider
{
    // The conventional suffix of services inheriting from IAddressUpdater may be omitted.
    private const string Suffix = "AddressUpdater";

    public ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        Discover(builder);
        return ValueTask.CompletedTask;
    }

    public void Discover(ShapeTableBuilder builder) => builder
        .Describe(AddressFieldEditorViewModel.ShapeType)
        .OnDisplaying(displaying =>
        {
            var metadata = displaying.Shape.Metadata;

            var type = metadata.Type;
            var alternates = metadata.Alternates;
            var differentiator = metadata.Differentiator;

            var addressUpdaterTypeNames = displaying
                .DisplayContext
                .ServiceProvider
                .GetServices<IAddressUpdater>()
                .Select(updater => updater.GetType().Name)
                .Distinct();

            foreach (var typeName in addressUpdaterTypeNames)
            {
                alternates.Add($"{type}__{typeName}");
                alternates.Add($"{differentiator}__{typeName}");

                if (typeName.EndsWithOrdinalIgnoreCase(Suffix))
                {
                    var typeNameWithoutSuffix = typeName[0..^Suffix.Length];
                    alternates.Add($"{type}__{typeNameWithoutSuffix}");
                    alternates.Add($"{differentiator}__{typeNameWithoutSuffix}");
                }
            }

            return Task.CompletedTask;
        });
}
