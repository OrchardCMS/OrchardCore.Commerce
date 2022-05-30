namespace OrchardCore.Commerce.Tests.UI;

public static class Config
{
    public static string GetAbsoluteApplicationAssemblyPath()
    {
        // The test assembly can be in a folder below the src and test folders (those should be in the repo root).
        var baseDirectory = File.Exists("SampleWebApp.dll")
            ? AppContext.BaseDirectory
            : Path.Combine(
                AppContext.BaseDirectory.Split(new[] { "src", "test" }, StringSplitOptions.RemoveEmptyEntries)[0],
                "src",
                "SampleWebApp",
                "bin",
                "Debug",
                "net6.0");

        return Path.Combine(baseDirectory, "SampleWebApp.dll");
    }
}
