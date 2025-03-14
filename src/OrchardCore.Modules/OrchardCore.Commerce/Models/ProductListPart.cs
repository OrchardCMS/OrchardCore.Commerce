using OrchardCore.ContentManagement;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Models;

[SuppressMessage(
    "Minor Code Smell",
    "S2094:Classes should not be empty",
    Justification = "This part doesn't store data.")]
public class ProductListPart : ContentPart
{
}
