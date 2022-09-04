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

    public override Task LoadingAsync(LoadContentContext context, PricePart instance)
    {
        var price = _moneyService.EnsureCurrency(instance.Price);
        instance.Price = price;

        // Migrate objects to use PriceField instead of PricePart.Amount.
        if (instance.Content.Price != null &&
            instance.Content.PriceField?.Amount.ToString() != instance.Content.Price.ToString())
        {
            instance.Content.PriceField = JObject.FromObject(new PriceField { Amount = price });
            instance.Content.Price = JObject.FromObject(price);

            _session.Save(instance.ContentItem);
        }

        return base.LoadingAsync(context, instance);
    }
}
