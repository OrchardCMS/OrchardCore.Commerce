using Lombiq.Tests.UI.Extensions;

namespace Lombiq.Tests.UI.Services;

public static class NavigationUITestContextExtensions
{
    /// <summary>
    /// Navigates to the new field page for the given <paramref name="contentType"/>.
    /// </summary>
    public static Task GoToAddFieldToContentTypeAsync(this UITestContext context, string contentType) =>
        context.GoToAdminRelativeUrlAsync($"/ContentTypes/AddFieldsTo/{contentType}");
}
