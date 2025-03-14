using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.ContentFields.Events;

/// <summary>
/// Events related to address fields.
/// </summary>
public interface IAddressFieldEvents
{
    /// <summary>
    /// Invoked during the update by <see cref="AddressFieldDisplayDriver"/>.
    /// </summary>
    public Task UpdatingAsync(
        AddressFieldViewModel viewModel,
        AddressField field,
        IUpdateModel updater,
        UpdateFieldEditorContext context);
}
