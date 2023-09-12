using Atata;
using Lombiq.Tests.UI.Extensions;
using OpenQA.Selenium;
using Shouldly;

namespace Lombiq.Tests.UI.Services;

public static class NotificationUITestContextExtenisons
{
    public static string GetErrorMessage(this UITestContext context, bool safely = false)
    {
        var by = By.ClassName("message-error").Safely(safely);
        return context.Get(by)?.Text?.Trim();
    }

    public static void ErrorMessageShouldNotExist(this UITestContext context) =>
        context.GetErrorMessage(safely: true).ShouldBeNull();
}
