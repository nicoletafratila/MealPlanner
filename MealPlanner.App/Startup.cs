using MealPlanner.App.Services;

namespace MealPlanner.App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<CampContext>();
            //services.AddScoped<ICampRepository, CampRepository>();

            //services.AddAutoMapper(Assembly.GetExecutingAssembly());
            //services.AddApiVersioning(opt =>
            //{
            //    opt.AssumeDefaultVersionWhenUnspecified = true;
            //    opt.DefaultApiVersion = new ApiVersion(1, 1);
            //    opt.ReportApiVersions = true;
            //    opt.ApiVersionReader = ApiVersionReader.Combine(
            //        new HeaderApiVersionReader("X-Version"),
            //        new QueryStringApiVersionReader("ver", "version"));

            //    //opt.ApiVersionReader = new UrlSegmentApiVersionReader();

            //    opt.Conventions.Controller<TalksController>()
            //        .HasApiVersions(new List<ApiVersion>() { new ApiVersion(1, 0), new ApiVersion(1, 1) })
            //            .Action(c => c.Delete(default(string), default(int)))
            //            .MapToApiVersion(1, 1);
            //});

            //services.AddControllers();

            // Add services to the container.
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<IRecipeBookApiConfig, RecipeBookApiConfig>();
            services.AddSingleton<IQuantityCalculator, QuantityCalculator>();

            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetRequiredService<IRecipeBookApiConfig>();
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });

            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetRequiredService<IRecipeBookApiConfig>();
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });

            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetRequiredService<IRecipeBookApiConfig>();
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
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
