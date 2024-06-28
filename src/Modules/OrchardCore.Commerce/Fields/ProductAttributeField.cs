using Lombiq.HelpfulLibraries.Common.Utilities;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Fields;

[SuppressMessage(
    "Minor Code Smell",
    "S2094:Classes should not be empty",
    Justification = "Intermediate ancestor class to group together attribute fields.")]
public abstract class ProductAttributeField : ContentField
{
}

/// <summary>
/// Adds the ability for a product to be modified with a set of attributes, in particular when added to a shopping cart.
/// </summary>
/// <remarks>
/// <para>Examples of attributes can be shirt sizes (S, M, L, XL), dimensions, etc.</para>
/// </remarks>
public abstract class ProductAttributeField<TSettings> : ProductAttributeField
    where TSettings : ProductAttributeFieldSettings, ICopier<TSettings>, new()
{
    public TSettings GetSettings(ContentPartFieldDefinition partFieldDefinition) =>
        partFieldDefinition.GetSettings<TSettings>();
}

/// <summary>
/// A Boolean product attribute.
/// </summary>
public class BooleanProductAttributeField : ProductAttributeField<BooleanProductAttributeFieldSettings>
{
}

/// <summary>
/// A numeric product attribute.
/// </summary>
public class NumericProductAttributeField : ProductAttributeField<NumericProductAttributeFieldSettings>
{
}

/// <summary>
/// A text product attribute, that may also have predefined values and may be used as enumeration or flags.
/// </summary>
public class TextProductAttributeField : ProductAttributeField<TextProductAttributeFieldSettings>
{
}
