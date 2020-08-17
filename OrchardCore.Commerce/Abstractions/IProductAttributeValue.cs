using System;
using System.Globalization;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeValue
    {
        string AttributeName { get; }
        object UntypedValue { get; }

        string Display(CultureInfo culture = null);

        public string Label
        {
            get
            {
                string[] splitName = AttributeName.Split('.');
                if (splitName.Length < 2) return AttributeName;
                return splitName[1];
            }
        }

        public string PartName
        {
            get
            {
                string[] splitName = AttributeName.Split('.');
                if (splitName.Length < 2) return null;
                return splitName[0];
            }
        }
    }
    public interface IProductAttributeValue<T> : IProductAttributeValue, IEquatable<IProductAttributeValue<T>>
    {
        T Value { get; }
    }
}