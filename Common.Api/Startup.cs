using AutoMapper;
using Common.Data.DataContext;
using Common.Data.Profiles;
using Common.Data.Repository;
using Common.Logging;
using Microsoft.AspNetCore.Http;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MealPlannerDbContext>(options =>
            {
                //options.UseInMemoryDatabase(databaseName: "MealPlannerInMemory");
                options.UseSqlServer(Configuration.GetConnectionString("MealPlanner"), x => x.MigrationsAssembly("MealPlanner.Api"));
                options.EnableSensitiveDataLogging();
            });

            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            services.AddSingleton<HttpContextAccessor>();
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<ProductProfile>();
                c.AddProfile<RecipeIngredientProfile>();
                c.AddProfile<ProductCategoryProfile>();
                c.AddProfile<MealPlanProfile>();
                c.AddProfile<RecipeProfile>();
                c.AddProfile<RecipeCategoryProfile>();
                c.AddProfile<UnitProfile>();
                c.AddProfile<ShoppingListProfile>();
                c.AddProfile<ShoppingListProductProfile>();
                c.AddProfile<ShopProfile>();
                c.AddProfile<ShopDisplaySequenceProfile>();
                c.AddProfile<LogProfile>();
                c.AddProfile<ApplicationUserProfile>();
            });
            services.AddSingleton(s => config.CreateMapper());

            services.AddScoped<ILoggerRepository, LoggerRepository>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddSingleton<RecipeBookApiConfig>();
            services.AddSingleton<MealPlannerApiConfig>();
            services.AddSingleton<MealPlannerWebConfig>();
            services.AddSingleton<IdentityApiConfig>();
            RegisterRepositories(services);
            RegisterServices(services);

            services.AddControllersWithViews();
            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }
    }
}