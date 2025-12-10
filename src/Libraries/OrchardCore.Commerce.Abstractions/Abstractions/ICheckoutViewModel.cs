using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Nothing to say besides what's already on the property names.")]
public interface ICheckoutViewModel : IPaymentViewModel, IShape
{
    string ShoppingCartId { get; }
    Amount GrossTotal { get; }
    IEnumerable<SelectListItem> Regions { get; }
    IEnumerable<Region> RegionData { get; set; }
    IDictionary<string, IDictionary<string, string>> Provinces { get; }
    string UserEmail { get; }
    bool IsInvalid { get; set; }
    bool ShouldIgnoreAddress { get; set; }
    IEnumerable<IShape> CheckoutShapes { get; }
}
