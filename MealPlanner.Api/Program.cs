using Serilog;

namespace MealPlanner.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            await SeedData.EnsureSeedDataAsync(scope);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
        }
    }
}