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
public interface IPaymentViewModel : IShape
{
    Amount SingleCurrencyTotal { get; }
    Amount NetTotal { get; }
    IDictionary<string, object> PaymentProviderData { get; }
    OrderPart OrderPart { get; }
}
