using AutoMapper;
using Blazored.SessionStorage;
using Microsoft.Extensions.Logging.Abstractions;
using Common.Data.DataContext;
using Common.Data.Profiles;
using Common.Data.Repository;
using Common.Http;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Core
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        private static bool IsDevelopment =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        protected virtual void RegisterServices(IServiceCollection services) { }

        protected virtual void RegisterRepositories(IServiceCollection services) { }

        protected virtual void RegisterTableConfigurationAssemblies(IServiceCollection services) { }

        protected virtual void ConfigureMapper(IMapperConfigurationExpression cfg) { }

        public void ConfigureServices(IServiceCollection services)
        {
            RegisterTableConfigurationAssemblies(services);

            services.AddDbContext<MealPlannerDbContext>(options =>
            {
                //options.UseInMemoryDatabase(databaseName: "MealPlannerInMemory");
                options.UseSqlServer(Configuration.GetConnectionString("MealPlanner"), x => x.MigrationsAssembly("MealPlanner.Api"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                if (IsDevelopment)
                    options.EnableSensitiveDataLogging();
            });

            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<LogProfile>();
                ConfigureMapper(c);
            }, NullLoggerFactory.Instance);
            services.AddSingleton(s => config.CreateMapper());

            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<ILoggerRepository, LoggerRepository>();
            services.AddScoped<ILoggerService, LoggerService>();
            RegisterRepositories(services);
            RegisterServices(services);

            services.AddControllersWithViews();
            services.AddBlazoredSessionStorage();
            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }
    }
}