using System.Reflection;
using System.Text;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Identity.Api
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Identity API",
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
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<MealPlannerDbContext>()
                    .AddDefaultTokenProviders();
            services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryClients(IdentityConfigs.GetClients())
                    .AddInMemoryApiResources(IdentityConfigs.GetApiResources())
                    .AddInMemoryApiScopes(IdentityConfigs.GetApiScopes())
                    .AddInMemoryIdentityResources(IdentityConfigs.GetIdentityResources())
                    .AddAspNetIdentity<ApplicationUser>();
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseSerilogRequestLogging();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}