using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public record ShoppingCartDisplayingEventContext(
    IList<Amount> Totals,
    IList<LocalizedHtmlString> Headers,
    IList<ShoppingCartLineViewModel> Lines,
    Address ShippingAddress,
    Address BillingAddress);
