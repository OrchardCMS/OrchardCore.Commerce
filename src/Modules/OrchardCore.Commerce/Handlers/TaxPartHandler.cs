using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class TaxPartHandler : ContentPartHandler<TaxPart>
{
    private readonly IMoneyService _moneyService;
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    public TaxPartHandler(
        IMoneyService moneyService,
        ISession session,
        IUpdateModelAccessor updateModelAccessor)
    {
        _moneyService = moneyService;
        _session = session;
        _updateModelAccessor = updateModelAccessor;
    }

    public override Task PublishingAsync(PublishContentContext context, TaxPart instance)
    {
        var taxRate = instance.TaxRate?.Value ?? 0;

        var isGrossPricePresent = instance.GrossPrice?.Amount.IsValid == true;
        var isTaxRatePresent = taxRate > 0;

        if (!isGrossPricePresent && !isTaxRatePresent)
        {
            return Task.CompletedTask;
        }

        if (isGrossPricePresent && isTaxRatePresent)
        {
            if (!instance.ContentItem.Has<TaxPart>())
            {
                _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                    nameof(instance.GrossPrice),
                    $"The content item, must have {nameof(PricePart)}");
            }

            var netMultiplier = 1 + (taxRate / 100);
            return Task.CompletedTask;
        }

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(instance.Sku),
            "SKU must be unique. A product with the given SKU already exists.");
        return Task.CompletedTask;
    }
}
