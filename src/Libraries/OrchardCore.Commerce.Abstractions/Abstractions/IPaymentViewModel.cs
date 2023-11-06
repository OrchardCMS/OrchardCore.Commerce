using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Nothing to say besides what's already on the property names.")]
public interface IPaymentViewModel
{
    Amount SingleCurrencyTotal { get; }
    Amount NetTotal { get; }
    IDictionary<string, object> PaymentProviderData { get; }
    OrderPart OrderPart { get; }
}
