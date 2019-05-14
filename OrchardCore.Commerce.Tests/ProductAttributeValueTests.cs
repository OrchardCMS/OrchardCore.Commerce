using System;
using OrchardCore.Commerce.Models;
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
    }
}
