using Lombiq.HelpfulLibraries.OrchardCore.ResourceManagement;
using OrchardCore.Commerce.Constants;

namespace OrchardCore.Commerce.Services;

public class ResourceFilters : IResourceFilterProvider
{
    public void AddResourceFilter(ResourceFilterBuilder builder) =>
        builder.Always().RegisterStylesheet(ResourceNames.ErrorNotification);
}
