using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using System;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Constants.ContentTypes;

namespace OrchardCore.Commerce.Handlers;

public class OrderHandler : ContentHandlerBase
{
    private readonly IHttpContextAccessor _hca;
    private readonly Lazy<IUserService> _userServiceLazy;
    public OrderHandler(
        IHttpContextAccessor hca,
        Lazy<IUserService> userServiceLazy)
    {
        _hca = hca;
        _userServiceLazy = userServiceLazy;
    }

    public override async Task UpdatingAsync(UpdateContentContext context)
    {
        if (context.ContentItem.As<OrderPart>() is not { } orderPart) return;

        var userService = _userServiceLazy.Value;

        if (_hca.IsCheckoutFrontEnd() && await userService.GetCurrentFullUserAsync(_hca) is { } user)
        {
            var isSame = orderPart.BillingAndShippingAddressesMatch.Value;

            await userService.AlterUserSettingAsync(user, UserAddresses, contentItem =>
            {
                var part = contentItem.ContainsKey(nameof(UserAddressesPart))
                    ? contentItem[nameof(UserAddressesPart)].ToObject<UserAddressesPart>()!
                    : new UserAddressesPart();
                    
                part.BillingAndShippingAddressesMatch.Value = isSame;
                contentItem[nameof(UserAddressesPart)] = JToken.FromObject(part);
                return contentItem;
            });
        }
    }
}
