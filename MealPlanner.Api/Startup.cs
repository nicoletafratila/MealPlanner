using AutoMapper;
using Common.Data.DataContext;
using Common.Profiles;
using Common.Repository.Repositories;
using MealPlanner.Api.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MealPlanner.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MealPlannerDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("MealPlanner"));
                options.EnableSensitiveDataLogging();
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<IngredientProfile>();
                c.AddProfile<MealPlanProfile>();
                c.AddProfile<RecipeProfile>();
            });
            services.AddSingleton<IMapper>(s => config.CreateMapper());

            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            services.AddScoped<IMealPlanRepository, MealPlanRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
