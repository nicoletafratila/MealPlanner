namespace MealPlanner.UI.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder);

            var app = builder.Build();
            startup.Configure(app, builder.Environment);
        }
    }
}