using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog;

namespace MealPlanner.UI.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            string fileLoggerFilePath = Path.Combine(currentDir, "Logs", "logs.log");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) =>
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

            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder);

            var app = builder.Build();
            startup.Configure(app, builder.Environment);
        }
    }
}