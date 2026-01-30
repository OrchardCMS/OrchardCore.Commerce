using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public abstract class CreatingOrUpdatingPartHandler<T> : ContentPartHandler<T>
    where T : ContentPart, new()
{
    protected abstract Task CreatingOrUpdatingAsync(T part);

    public override Task CreatingAsync(CreateContentContext context, T part) =>
        CreatingOrUpdatingAsync(part);

    public override Task UpdatingAsync(UpdateContentContext context, T part) =>
        CreatingOrUpdatingAsync(part);
}
