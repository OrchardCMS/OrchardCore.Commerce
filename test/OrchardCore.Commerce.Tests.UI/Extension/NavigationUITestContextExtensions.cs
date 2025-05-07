using Lombiq.Tests.UI.Extensions;

namespace Lombiq.Tests.UI.Services;

internal static class NavigationUITestContextExtensions
{
    /// <summary>
    /// Navigates to the new field page for the given <paramref name="contentType"/>.
    /// </summary>
    public static Task GoToAddFieldToContentTypeAsync(this UITestContext context, string contentType) =>
        context.GoToAdminRelativeUrlAsync($"/ContentTypes/AddFieldsTo/{contentType}");

    /// <summary>
    /// Navigates to the cart./>.
    /// </summary>
    public static Task GoToCartAsync(this UITestContext context) =>
        context.GoToRelativeUrlAsync("/cart");

    /// <summary>
    /// Navigates to the checkout page./>.
    /// </summary>
    public static Task GoToCheckoutAsync(this UITestContext context) =>
        context.GoToRelativeUrlAsync("/checkout");
}
