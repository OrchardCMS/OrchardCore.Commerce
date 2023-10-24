using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Abstractions;

[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Nothing to say besides what's already on the property names.")]
public interface ICheckoutViewModel : IShape
{
    string ShoppingCartId { get; }
    Amount SingleCurrencyTotal { get; }
    Amount NetTotal { get; }
    Amount GrossTotal { get; }
    OrderPart OrderPart { get; }
    string PaymentIntentClientSecret { get; }
    IEnumerable<SelectListItem> Regions { get; set; }
    IDictionary<string, IDictionary<string, string>> Provinces { get; }
    string StripePublishableKey { get; }
    string UserEmail { get; }
    IEnumerable<IShape> CheckoutShapes { get; }
}
