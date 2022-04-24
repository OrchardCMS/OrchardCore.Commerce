namespace OrchardCore.Commerce.Abstractions;

public interface IPredefinedValuesProductAttributeValue : IProductAttributeValue
{
    public object UntypedPredefinedValue { get; }
}

public interface IPredefinedValuesProductAttributeValue<out T> : IPredefinedValuesProductAttributeValue
{
    public T PredefinedValue { get; }
}
