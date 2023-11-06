namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Represents a predefined <see cref="IProductAttributeValue"/> with no type specified.
/// </summary>
public interface IPredefinedValuesProductAttributeValue : IProductAttributeValue
{
    /// <summary>
    /// Gets a predefined value a product attribute field can take.
    /// </summary>
    public object UntypedPredefinedValue { get; }
}

/// <summary>
/// Represents a predefined <see cref="IProductAttributeValue"/> with of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="PredefinedValue"/>.</typeparam>
public interface IPredefinedValuesProductAttributeValue<out T> : IPredefinedValuesProductAttributeValue
{
    /// <summary>
    /// Gets a predefined value a product attribute field can take.
    /// </summary>
    public T PredefinedValue { get; }
}
