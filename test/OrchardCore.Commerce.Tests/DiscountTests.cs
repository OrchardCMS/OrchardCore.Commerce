using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.Commerce.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public sealed class DiscountTests
{
    [Fact]
    public async Task PromotionServiceAddsDiscount()
    {
        var promotionService = new DummyPromotionService(new FakeDiscountProvider());

        var viewModelLineItems = new List<OrderLineItemViewModel>
            {
                new()
                {
                    ProductPart = null,
                    Quantity = 10,
                    ProductSku = "test",
                    ProductName = "Test",
                    UnitPrice = CreateUsDollarAmount(10),
                    LinePrice = CreateUsDollarAmount(100),
                    ProductRouteValues = null,
                    Attributes = null,
                },
            };

        var total = viewModelLineItems.Select(item => item.LinePrice).Sum();

        var promotionAndTaxContext = new PromotionAndTaxProviderContext(
            viewModelLineItems.Select(item => new PromotionAndTaxProviderContextLineItem(
                Content: null,
                item.UnitPrice,
                item.Quantity,
                [])),
            [total],
            ShippingAddress: null,
            BillingAddress: null);

        promotionAndTaxContext = await promotionService.AddPromotionsAsync(promotionAndTaxContext);

        var firstAndOnlyItem = promotionAndTaxContext.Items.First();

        Assert.Equal(CreateUsDollarAmount(5), firstAndOnlyItem.UnitPrice);
        Assert.Equal(CreateUsDollarAmount(50), firstAndOnlyItem.Subtotal);
        Assert.Equal(10, firstAndOnlyItem.Quantity);
    }

    private sealed class DummyPromotionService : IPromotionService
    {
        private readonly IPromotionProvider _promotionProvider;

        // Keeping it simple, only using one provider instead of a collection.
        public DummyPromotionService(IPromotionProvider promotionProviders) =>
            _promotionProvider = promotionProviders;

        // Keeping it simple, no ordering or "is applicable" check.
        public Task<PromotionAndTaxProviderContext> AddPromotionsAsync(PromotionAndTaxProviderContext context) =>
            _promotionProvider.UpdateAsync(context);

        // IPromotionService's method needs to be created, but implementation is unnecessary as the tests do not use it.
        public Task<bool> IsThereAnyApplicableProviderAsync(PromotionAndTaxProviderContext context) =>
            throw new NotSupportedException();
    }

    private static Amount CreateUsDollarAmount(decimal value) =>
        new(value, Currency.UsDollar);
}
