using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.Tests.Fakes;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public partial class ProductAttributeTests
    {
        private IProductAttributeProvider parser;
        private ContentPartDefinition partDefinition;
        private ContentPartFieldDefinition boolFieldDefinition;
        private ContentPartFieldDefinition numericFieldDefinition;
        private ContentPartFieldDefinition textFieldDefinition;
        private ContentTypePartDefinition partTypeDefinition;

        public ProductAttributeTests()
        {
            parser = new ProductAttributeProvider();
            partDefinition = new ContentPartDefinition("Product");
            boolFieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(BooleanProductAttributeField)),
                "BooleanField", new JObject())
            { PartDefinition = partDefinition };
            numericFieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(NumericProductAttributeField)),
                "NumericField", new JObject())
            { PartDefinition = partDefinition };
            textFieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(TextProductAttributeField)),
                "TextField", new JObject())
            { PartDefinition = partDefinition };
            partTypeDefinition = new ContentTypePartDefinition("product", partDefinition, new JObject());
        }

        [Fact]
        public void BooleanAttributeEquality()
        {
            var trueValue = new BooleanProductAttributeValue("true", true);
            var otherTrue = new BooleanProductAttributeValue("other", true);
            var falseValue = new BooleanProductAttributeValue("false", false);
            var otherFalse = new BooleanProductAttributeValue("other", false);

            Assert.True(trueValue.Equals(trueValue));
            Assert.True(falseValue.Equals(falseValue));
            Assert.True(trueValue.Equals(new BooleanProductAttributeValue("true", true)));

            Assert.False(trueValue.Equals(otherTrue));
            Assert.False(trueValue.Equals(falseValue));
            Assert.False(otherTrue.Equals(otherFalse));
            Assert.False(trueValue.Equals(null));
        }

        [Fact]
        public void BooleanAttributeParse()
        {
            var trueValue = parser.Parse(partTypeDefinition, boolFieldDefinition, "true") as BooleanProductAttributeValue;
            var falseValue = parser.Parse(partTypeDefinition, boolFieldDefinition, "false") as BooleanProductAttributeValue;

            Assert.NotNull(trueValue);
            Assert.Equal("product.BooleanField", trueValue.AttributeName);
            Assert.True(trueValue.Value);

            Assert.NotNull(falseValue);
            Assert.Equal("product.BooleanField", falseValue.AttributeName);
            Assert.False(falseValue.Value);
        }

        [Fact]
        public void NumericAttributeEquality()
        {
            var oneValue = new NumericProductAttributeValue("one", 1);
            var otherOne = new NumericProductAttributeValue("other", 1);
            var twoValue = new NumericProductAttributeValue("two", 2);

            Assert.True(oneValue.Equals(oneValue));
            Assert.True(twoValue.Equals(twoValue));
            Assert.True(oneValue.Equals(new NumericProductAttributeValue("one", 1)));

            Assert.False(oneValue.Equals(otherOne));
            Assert.False(oneValue.Equals(twoValue));
            Assert.False(oneValue.Equals(null));
        }

        [Fact]
        public void NumericAttributeParse()
        {
            var oneValue = parser.Parse(partTypeDefinition, numericFieldDefinition, "1") as NumericProductAttributeValue;
            var twoValue = parser.Parse(partTypeDefinition, numericFieldDefinition, "2") as NumericProductAttributeValue;

            Assert.NotNull(oneValue);
            Assert.Equal("product.NumericField", oneValue.AttributeName);
            Assert.Equal(1, oneValue.Value);

            Assert.NotNull(twoValue);
            Assert.Equal("product.NumericField", twoValue.AttributeName);
            Assert.Equal(2, twoValue.Value);
        }

        [Fact]
        public void TextAttributeEquality()
        {
            var oneValue = new TextProductAttributeValue("one", "1");
            var otherOne = new TextProductAttributeValue("other", "1");
            var twoValue = new TextProductAttributeValue("two", "2");

            Assert.True(oneValue.Equals(oneValue));
            Assert.True(twoValue.Equals(twoValue));
            Assert.True(oneValue.Equals(new TextProductAttributeValue("one", "1")));

            Assert.False(oneValue.Equals(otherOne));
            Assert.False(oneValue.Equals(twoValue));
            Assert.False(oneValue.Equals(null));
        }

        [Fact]
        public void TextAttributeMultipleValuesEquality()
        {
            var oneTwo = new TextProductAttributeValue("", "1", "2");
            var oneTwoThree = new TextProductAttributeValue("", "1", "2", "3");
            var twoOneThree = new TextProductAttributeValue("", "2", "1", "3");

            Assert.True(oneTwo.Equals(oneTwo));
            Assert.True(twoOneThree.Equals(twoOneThree));
            Assert.True(oneTwoThree.Equals(twoOneThree));
            Assert.True(twoOneThree.Equals(oneTwoThree));
            Assert.True(oneTwo.Equals(new TextProductAttributeValue("", "1", "2")));

            Assert.False(oneTwo.Equals(oneTwoThree));
            Assert.False(oneTwo.Equals(twoOneThree));
            Assert.False(oneTwo.Equals(null));
        }

        [Fact]
        public void TextAttributeParse()
        {
            var oneValue = parser.Parse(partTypeDefinition, textFieldDefinition, "1") as TextProductAttributeValue;
            var twoValue = parser.Parse(partTypeDefinition, textFieldDefinition, "2") as TextProductAttributeValue;
            var listValue = parser.Parse(partTypeDefinition, textFieldDefinition, new[] { "1", "2", "3" }) as TextProductAttributeValue;

            Assert.NotNull(oneValue);
            Assert.Equal("product.TextField", oneValue.AttributeName);
            Assert.Equal(new[] { "1" }, oneValue.Value);

            Assert.NotNull(twoValue);
            Assert.Equal("product.TextField", twoValue.AttributeName);
            Assert.Equal(new[] { "2" }, twoValue.Value);

            Assert.NotNull(listValue);
            Assert.Equal("product.TextField", listValue.AttributeName);
            Assert.Equal(new[] { "1", "2", "3" }, listValue.Value);
        }

        [Fact]
        public void ProductAttributeServiceCanFindAttributesOnProducts()
        {
            var productAttributeService = new ProductAttributeService(null, new FakeContentDefinitionManager(), new FakeFieldOptions(), null);
            var product = new ContentItem() {
                ContentType = "Product"
            };
            var productPart1 = new ContentPart { };
            var boolProductAttribute = new BooleanProductAttributeField();
            productPart1.Weld("foobool", boolProductAttribute);
            productPart1.Weld("barbool", new BooleanField());
            product.Weld("ProductPart1", productPart1);
            var productPart2 = new ContentPart { };
            var textProductAttribute = new TextProductAttributeField();
            productPart2.Weld("footext", textProductAttribute);
            productPart2.Weld("bartext", new TextField());
            product.Weld("ProductPart2", productPart2);

            var productAttributeFields = productAttributeService.GetProductAttributeFields(product).ToArray();

            Assert.Equal(2, productAttributeFields.Length);
            var foobool = productAttributeFields.FirstOrDefault(f => f.Name == "foobool");
            Assert.Equal("ProductPart1", foobool.PartName);
            Assert.Equal(boolProductAttribute, foobool.Field);
            Assert.IsType<BooleanProductAttributeFieldSettings>(foobool.Settings);
            var footext = productAttributeFields.FirstOrDefault(f => f.Name == "footext");
            Assert.Equal("ProductPart2", footext.PartName);
            Assert.Equal(textProductAttribute, footext.Field);
            Assert.IsType<TextProductAttributeFieldSettings>(footext.Settings);
        }
    }
}
