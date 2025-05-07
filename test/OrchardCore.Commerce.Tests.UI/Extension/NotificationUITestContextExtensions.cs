using Atata;
using Lombiq.Tests.UI.Extensions;
using OpenQA.Selenium;
using Shouldly;

namespace Lombiq.Tests.UI.Services;

internal static class NotificationUITestContextExtensions
{
    /// <summary>
    /// Returns the text of the element with the <c>message-error</c> class if one exists.
    /// </summary>
    /// <param name="safely">
    /// If the element is found then this doesn't matter. Otherwise if it's <see langword="true"/> then <see
    /// langword="null"/> is returned and if it's <see langword="false"/> an exception is thrown.
    /// </param>
    public static string GetErrorMessage(this UITestContext context, bool safely = false)
    {
        var by = By.ClassName("message-error").Safely(safely);
        return context.Get(by)?.Text?.Trim();
    }

    /// <summary>
    /// Looks for the element with the <c>message-error</c> class, it shouldn't exist or its content should be empty. If
    /// that's not true an exception will be thrown containing the element text.
    /// </summary>
    public static void ErrorMessageShouldNotExist(this UITestContext context) =>
        context.GetErrorMessage(safely: true).ShouldBeNullOrEmpty();
}
