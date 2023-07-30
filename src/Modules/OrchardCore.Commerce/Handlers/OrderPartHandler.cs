using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Models;
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

    public override Task UpdatedAsync(UpdateContentContext context, OrderPart part)
    {
        var guid = string.IsNullOrEmpty(part.OrderId.Text) ? Guid.NewGuid().ToString() : part.OrderId.Text;
        part.OrderId.Text = guid;

        part.ContentItem.DisplayText = T["Order {0}", guid];

        part.Apply();
        _session.Save(part.ContentItem);

        return Task.CompletedTask;
    }
}
