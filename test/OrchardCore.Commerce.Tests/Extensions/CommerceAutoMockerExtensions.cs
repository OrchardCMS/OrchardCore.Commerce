using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.ContentManagement.Metadata;
using System.Collections.Generic;

namespace Moq.AutoMock;

public static class CommerceAutoMockerExtensions
{
    public static void UseCommerceFakes(this AutoMocker mocker)
    {
        mocker.Use<IPriceService>(new FakePriceService());
        mocker.Use<IProductService>(new FakeProductService());
        mocker.Use<IEnumerable<IShoppingCartEvents>>([new FakeShoppingCartEvents()]);
        mocker.Use<IContentDefinitionManager>(new FakeContentDefinitionManager());
        mocker.Use<IMoneyService>(new TestMoneyService());
    }

    public static void UseProductAttributeProvider(this AutoMocker mocker) =>
        mocker.Use<IEnumerable<IProductAttributeProvider>>([new ProductAttributeProvider()]);

    public static IShoppingCartSerializer CreateShoppingCartSerializerInstance(this AutoMocker mocker)
    {
        mocker.UseCommerceFakes();
        mocker.UseProductAttributeProvider();

        return mocker.CreateInstance<ShoppingCartSerializer>();
    }

    public static IProductAttributeService CreateProductAttributeServiceInstance(this AutoMocker mocker)
    {
        mocker.UseCommerceFakes();
        return mocker.CreateInstance<ProductAttributeService>();
    }
}
