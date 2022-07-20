using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Logging;

namespace OrchardCore.Commerce.Web;

public static class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging => logging.ClearProviders())
        .ConfigureWebHostDefaults(webBuilder => webBuilder
            .UseNLogWeb()
            .UseStartup<Startup>());
}
