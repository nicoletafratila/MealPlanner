using AutoMapper;
using Common.Data.DataContext;
using Common.Data.Profiles;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Common.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected virtual void RegisterServices(IServiceCollection services) { }

        protected virtual void RegisterRepositories(IServiceCollection services) { }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MealPlannerDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("MealPlanner"), x => x.MigrationsAssembly("Common.Data.DataContext"));
                options.EnableSensitiveDataLogging();
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<IngredientProfile>();
                c.AddProfile<RecipeIngredientProfile>();
                c.AddProfile<IngredientCategoryProfile>();
                c.AddProfile<MealPlanProfile>();
                c.AddProfile<RecipeProfile>();
                c.AddProfile<RecipeCategoryProfile>();
                c.AddProfile<UnitProfile>();
            });
            services.AddSingleton(s => config.CreateMapper());

            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            RegisterRepositories(services);
            RegisterServices(services);

            services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors("Open");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
