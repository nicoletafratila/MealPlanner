using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Identity.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            await SeedData.EnsureSeedDataAsync(scope);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            string fileLoggerFilePath = Path.Combine(currentDir, "Logs", "logs.log");

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog((ctx, lc) =>
                {
                    var connectionString = ctx.Configuration.GetConnectionString("MealPlanner");

                    lc.MinimumLevel.Error()
                      .Enrich.FromLogContext()
                      .ReadFrom.Configuration(ctx.Configuration)
                      .WriteTo.Console(
                          restrictedToMinimumLevel: LogEventLevel.Error,
                          outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                      .WriteTo.File(
                          fileLoggerFilePath,
                          restrictedToMinimumLevel: LogEventLevel.Error,
                          rollingInterval: RollingInterval.Hour,
                          encoding: System.Text.Encoding.UTF8)
                      .WriteTo.MSSqlServer(
                          connectionString,
                          sinkOptions: new MSSqlServerSinkOptions
                          {
                              TableName = "Logs",
                              SchemaName = "dbo"
                          },
                          restrictedToMinimumLevel: LogEventLevel.Error);
                });
        }
    }
}