using System.Reflection;
using MediatR;
using RecipeBook.Api.Repositories;
using Serilog;

namespace RecipeBook.Api
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IRecipeIngredientRepository, RecipeIngredientRepository>();
            services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors("Open");
            app.UseStaticFiles();
            app.UseRouting();
            //app.UseIdentityServer();
            //app.UseAuthentication();
            //app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
