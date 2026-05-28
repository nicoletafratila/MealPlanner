using AutoMapper;
using Blazored.SessionStorage;
using Common.Data.DataContext;
using Common.Data.Profiles;
using Common.Data.Repository;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Api
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        protected virtual void RegisterServices(IServiceCollection services) { }

        protected virtual void RegisterRepositories(IServiceCollection services) { }

        protected virtual void ConfigureMapper(IMapperConfigurationExpression cfg) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MealPlannerDbContext>(options =>
            {
                //options.UseInMemoryDatabase(databaseName: "MealPlannerInMemory");
                options.UseSqlServer(Configuration.GetConnectionString("MealPlanner"), x => x.MigrationsAssembly("MealPlanner.Api"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.EnableSensitiveDataLogging();
            });

            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<LogProfile>();
                ConfigureMapper(c);
            });
            services.AddSingleton(s => config.CreateMapper());

            services.AddScoped<TokenProvider>();
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