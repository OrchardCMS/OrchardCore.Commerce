using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement.Metadata.Models;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class ProductAttributeValueTests
    {
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
            var parser = new ProductAttributeParseService();
            var fieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(BooleanProductAttributeField)),
                "BooleanField", new JObject());
            var trueValue = parser.Parse(fieldDefinition, "true") as BooleanProductAttributeValue;
            var falseValue = parser.Parse(fieldDefinition, "false") as BooleanProductAttributeValue;

            Assert.NotNull(trueValue);
            Assert.Equal("BooleanField", trueValue.AttributeName);
            Assert.True(trueValue.Value);

            Assert.NotNull(falseValue);
            Assert.Equal("BooleanField", falseValue.AttributeName);
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
            var parser = new ProductAttributeParseService();
            var fieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(NumericProductAttributeField)),
                "NumericField", new JObject());
            var oneValue = parser.Parse(fieldDefinition, "1") as NumericProductAttributeValue;
            var twoValue = parser.Parse(fieldDefinition, "2") as NumericProductAttributeValue;

            Assert.NotNull(oneValue);
            Assert.Equal("NumericField", oneValue.AttributeName);
            Assert.Equal(1, oneValue.Value);

            Assert.NotNull(twoValue);
            Assert.Equal("NumericField", twoValue.AttributeName);
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
            var parser = new ProductAttributeParseService();
            var fieldDefinition = new ContentPartFieldDefinition(
                new ContentFieldDefinition(nameof(TextProductAttributeField)),
                "TextField", new JObject());
            var oneValue = parser.Parse(fieldDefinition, "1") as TextProductAttributeValue;
            var twoValue = parser.Parse(fieldDefinition, "2") as TextProductAttributeValue;
            var listValue = parser.Parse(fieldDefinition, "1,2,3") as TextProductAttributeValue;

            Assert.NotNull(oneValue);
            Assert.Equal("TextField", oneValue.AttributeName);
            Assert.Equal(new[] { "1" }, oneValue.Value);

            Assert.NotNull(twoValue);
            Assert.Equal("TextField", twoValue.AttributeName);
            Assert.Equal(new[] { "2" }, twoValue.Value);

            Assert.NotNull(listValue);
            Assert.Equal("TextField", listValue.AttributeName);
            Assert.Equal(new[] { "1", "2", "3" }, listValue.Value);
        }
    }
}
