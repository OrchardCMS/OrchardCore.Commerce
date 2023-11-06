using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public record ShoppingCartDisplayingEventContext(
    IList<Amount> Totals,
    IList<LocalizedHtmlString> Headers,
    IList<ShoppingCartLineViewModel> Lines,
    Address ShippingAddress,
    Address BillingAddress);
