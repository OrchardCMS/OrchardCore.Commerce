namespace OrchardCore.Commerce.Payment.Exactly.Models;

public class ExactlyRequest<T>
    where T : IExactlyRequestAttributes
{
    public string Type => Attributes?.Type;
    public T Attributes { get; set; }
}
