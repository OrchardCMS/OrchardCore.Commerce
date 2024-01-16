using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class AddressUpdaterShapeTableProvider : IShapeTableProvider
{
    // The conventional suffix of services inheriting from IAddressUpdater may be omitted.
    private const string Suffix = "AddressUpdater";

    private readonly IEnumerable<IAddressUpdater> _addressUpdaters;

    public AddressUpdaterShapeTableProvider(IEnumerable<IAddressUpdater> addressUpdaters) =>
        _addressUpdaters = addressUpdaters;

    public void Discover(ShapeTableBuilder builder) => builder
        .Describe(AddressFieldEditorViewModel.ShapeType)
        .OnDisplaying(displaying =>
        {
            var metadata = displaying.Shape.Metadata;

            var type = metadata.Type;
            var alternates = metadata.Alternates;
            var differentiator = metadata.Differentiator;

            foreach (var typeName in _addressUpdaters.Select(updater => updater.GetType().Name).Distinct())
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
