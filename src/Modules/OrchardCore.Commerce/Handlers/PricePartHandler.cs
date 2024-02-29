using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class PricePartHandler : ContentPartHandler<PricePart>
{
    private readonly IMoneyService _moneyService;
    private readonly ISession _session;

    public PricePartHandler(IMoneyService moneyService, ISession session)
    {
        _moneyService = moneyService;
        _session = session;
    }

    public override async Task LoadingAsync(LoadContentContext context, PricePart part)
    {
        var amount = _moneyService.EnsureCurrency(part.Price);
        part.Price = amount;

        // Migrate objects to use PriceField instead of PricePart.Amount.
        if (part.Content.Price is { } price && part.Content.PriceField?.Amount.ToString() != price.ToString())
        {
            part.Content.PriceField = JObject.FromObject(new PriceField { Amount = amount });
            ((JObject)part.Content).Remove(nameof(PricePart.Price));

            await _session.SaveAsync(part.ContentItem);
        }

        await base.LoadingAsync(context, part);
    }
}
