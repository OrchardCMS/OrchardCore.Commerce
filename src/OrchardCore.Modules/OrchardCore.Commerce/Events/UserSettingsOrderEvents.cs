﻿using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static OrchardCore.Commerce.ContentFields.Constants.ContentTypes;

namespace OrchardCore.Commerce.Events;

public class UserSettingsOrderEvents : IOrderEvents
{
    private readonly IHttpContextAccessor _hca;
    private readonly IUserService _userService;

    public UserSettingsOrderEvents(IHttpContextAccessor hca, IUserService userService)
    {
        _hca = hca;
        _userService = userService;
    }

    public async Task FinalizeAsync(ContentItem order, string shoppingCartId, string paymentProviderName)
    {
        // Saving addresses.
        var orderPart = order.As<OrderPart>();

        if (_hca.HttpContext != null && await _userService.GetFullUserAsync(_hca.HttpContext.User) is { } user)
        {
            var isSame = orderPart.BillingAndShippingAddressesMatch.Value;

            await _userService.AlterUserSettingAsync(user, UserAddresses, contentItem =>
            {
                var part = contentItem[nameof(UserAddressesPart)] is { } partJson
                    ? partJson.ToObject<UserAddressesPart>()!
                    : new UserAddressesPart();

                part.BillingAndShippingAddressesMatch.Value = isSame;
                contentItem[nameof(UserAddressesPart)] = JObject.FromObject(part);
                return contentItem;
            });
        }
    }
}
