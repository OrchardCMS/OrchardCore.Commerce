using OrchardCore.Commerce.Payment.Abstractions;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Services;

public class StripeOrderContentTypeDefinitionExclusionProvider : IOrderContentTypeDefinitionExclusionProvider
{
    private static readonly string[] _excludedShapes =
    {
        "Order_Checkout__StripePaymentPart__PaymentIntentId",
        "Order_Checkout__StripePaymentPart__PaymentMethodId",
    };

    public IEnumerable<string> GetExcludedShapes(
        IEnumerable<(string ShapeType, Uri Url, bool IsNew)> templateLinks) =>
        _excludedShapes;
}
