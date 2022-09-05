using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Tax.Handlers;

public class TaxPartHandler : ContentPartHandler<TaxPart>
{
    private readonly IMoneyService _moneyService;
    private readonly ISession _session;
    public TaxPartHandler(IMoneyService moneyService, ISession session)
    {
        _moneyService = moneyService;
        _session = session;

    }

    public override Task PublishingAsync(PublishContentContext context, TaxPart instance)
    {
        return base.PublishingAsync(context, instance);
    }
}
