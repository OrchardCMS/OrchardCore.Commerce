using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;
using static OrchardCore.Commerce.Services.DiscountProvider;
using ISession = YesSql.ISession;

namespace OrchardCore.Commerce.Services;

public class GlobalDiscountProvider : IPromotionProvider
{
    public const string StereotypeName = "GlobalDiscount";

    private readonly IAuthorizationService _authorizationService;
    private readonly IClock _clock;
    private readonly IContentDefinitionService _contentDefinitionService;
    private readonly IHttpContextAccessor _hca;
    private readonly ISession _session;

    public int Order => 1;

    public GlobalDiscountProvider(
        IAuthorizationService authorizationService,
        IClock clock,
        IContentDefinitionService contentDefinitionService,
        IHttpContextAccessor hca,
        ISession session)
    {
        _authorizationService = authorizationService;
        _clock = clock;
        _contentDefinitionService = contentDefinitionService;
        _hca = hca;
        _session = session;
    }

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model) =>
        model.UpdateAsync(async (item, purchaseDateTime) =>
            ApplyPromotionToShoppingCartItem(
                item,
                purchaseDateTime,
                await QueryDiscountPartsAsync(model)));

    public async Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        (await QueryDiscountPartsAsync(model)).Any();

    private async Task<IEnumerable<DiscountPart>> QueryDiscountPartsAsync(PromotionAndTaxProviderContext model)
    {
        var typeNames = _contentDefinitionService.GetTypes()
            .Where(model => model.Settings["Stereotype"]?.ToString().EqualsOrdinalIgnoreCase(StereotypeName) == true)
            .Select(model => model.Name)
            .ToList();

        var globalDiscountItems = await _session
            .Query<ContentItem, ContentItemIndex>(index => index.ContentType.IsIn(typeNames))
            .ListAsync();

        globalDiscountItems = await globalDiscountItems.WhereAsync(item =>
            _authorizationService.AuthorizeAsync(_hca.HttpContext!.User, CommonPermissions.ViewContent, item));

        int totalQuantity = model.Items.Sum(item => item.Quantity);
        return globalDiscountItems
            .As<DiscountPart>()
            .Where(part => part.IsApplicable(totalQuantity, model.PurchaseDateTime ?? _clock.UtcNow));
    }
}
