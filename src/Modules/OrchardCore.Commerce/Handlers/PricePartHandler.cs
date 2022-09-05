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
        var amount = _moneyService.EnsureCurrency(instance.Price);
        instance.Price = amount;

        // Migrate objects to use PriceField instead of PricePart.Amount.
        if (instance.Content.Price is { } price && instance.Content.PriceField?.Amount.ToString() != price.ToString())
        {
            instance.Content.PriceField = JObject.FromObject(new PriceField { Amount = amount });
            ((JObject)instance.Content).Remove(nameof(PricePart.Price));

            _session.Save(instance.ContentItem);
        }

        return base.LoadingAsync(context, instance);
    }
}
