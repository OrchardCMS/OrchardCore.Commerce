using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using System;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class OrderPartHandler : ContentPartHandler<OrderPart>
{
    private readonly IStringLocalizer T;
    private readonly ISession _session;

    public OrderPartHandler(IStringLocalizer<OrderPartHandler> stringLocalizer, ISession session)
    {
        _session = session;
        T = stringLocalizer;
    }

    public override async Task UpdatedAsync(UpdateContentContext context, OrderPart part)
    {
        if (part.ContentItem.As<OrderPart>() is not { } orderPart) return;

        var guid = orderPart.OrderId.Text ?? Guid.NewGuid().ToString();
        orderPart.OrderId.Text = guid;

        orderPart.ContentItem.DisplayText = T["Order {0}", guid];

        orderPart.Apply();
        await _session.SaveAsync(orderPart.ContentItem);

        return;
    }
}
