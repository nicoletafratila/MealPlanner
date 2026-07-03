using System.Reflection;
using System.Text;
using AutoMapper;
using Common.Data.DataContext;
using Duende.IdentityModel;
using MealPlanner.Data.Profiles;
using MealPlanner.Data.TableConfigurations;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecipeBook.Api.Abstractions;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Profiles;
using RecipeBook.Data.TableConfigurations;
using Serilog;

namespace RecipeBook.Api
{
    public class Startup(IConfiguration configuration) : Common.Core.Startup(configuration)
    {
        protected override void RegisterTableConfigurationAssemblies(IServiceCollection services)
        {
            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));
        }

        protected override void ConfigureMapper(IMapperConfigurationExpression cfg)
        {
            cfg.AddProfile<ProductProfile>();
            cfg.AddProfile<RecipeIngredientProfile>();
            cfg.AddProfile<ProductCategoryProfile>();
            cfg.AddProfile<RecipeProfile>();
            cfg.AddProfile<RecipeCategoryProfile>();
            cfg.AddProfile<UnitProfile>();
            cfg.AddProfile<ShopProfile>();
            cfg.AddProfile<ShopDisplaySequenceProfile>();
            cfg.AddProfile<ShoppingListProductProfile>();
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<MealPlannerClientConfig>();

            services.AddHttpClient<IMealPlannerClient, MealPlannerClient>()
                .ConfigureHttpClient(ConfigureMealPlannerClient); ;

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "RecipeBook API",
                    Version = "v1"
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer {token}'",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                };

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        securityScheme,
                        new[] { Common.Constants.MealPlanner.ApiScope }
                    }
                });
            });
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IRecipeIngredientRepository, RecipeIngredientRepository>();
            services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                })
                .AddCookie(IdentityConstants.ApplicationScheme, options =>
                {
                    options.LoginPath = "/identities/login";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Common.Constants.MealPlanner.Issuer,
                        ValidAudience = Common.Constants.MealPlanner.ApiScope,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey)),
                    };
                });
            services.AddAuthorizationBuilder()
                .AddPolicy(Common.Constants.MealPlanner.PolicyScope, policy =>
                {
                    policy.AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme,
                        IdentityConstants.ApplicationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Scope, Common.Constants.MealPlanner.ApiScope);
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RecipeBook API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseSerilogRequestLogging();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureMealPlannerClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var clientConfig = serviceProvider.GetRequiredService<MealPlannerClientConfig>();
            httpClient.BaseAddress = clientConfig.BaseUrl;
            httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
        }
    }
}