using Common.Http;
using CommunityToolkit.Maui;
using Identity.Services.Http;
using MealPlanner.Services.Http;
using MealPlanner.UI.Mobile.Pages;
using MealPlanner.UI.Mobile.Pages.Identity;
using MealPlanner.UI.Mobile.Pages.MealPlans;
using MealPlanner.UI.Mobile.Pages.RecipeBook;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using RecipeBook.Services.Http;

namespace MealPlanner.UI.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit();

            // Load appsettings.json embedded resource
            var assembly = typeof(MauiProgram).Assembly;
            using var stream = assembly.GetManifestResourceStream("MealPlanner.UI.Mobile.appsettings.json");
            if (stream is not null)
            {
                var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
                builder.Configuration.AddConfiguration(config);
            }

            var identityBase = builder.Configuration["IdentityApi:BaseUrl"]!;
            var recipeBase = builder.Configuration["RecipeBookApi:BaseUrl"]!;
            var mealPlannerBase = builder.Configuration["MealPlannerApi:BaseUrl"]!;

            void ConfigureClient(HttpClient client, string baseUrl, string section)
            {
                client.BaseAddress = new Uri(baseUrl);
                if (builder.Configuration.GetValue<int>($"{section}:Timeout") is > 0 and var timeout)
                    client.Timeout = TimeSpan.FromSeconds(timeout);
            }

            var services = builder.Services;

            // Infrastructure
            services.AddMemoryCache();
            services.AddSingleton<SecureStorageTokenProvider>();
            services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<SecureStorageTokenProvider>());
            services.AddSingleton<AuthenticationStateService>();
            services.AddTransient<AuthRefreshHandler>();
            services.AddTransient<TimingHandler>();

#if DEBUG
            // Android emulator can't validate the .NET dev cert — bypass SSL in debug builds only
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(b =>
                    b.PrimaryHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }));
#endif

            // API HTTP clients
            services.AddHttpClient<IAuthenticationService, AuthenticationService>(c => ConfigureClient(c, identityBase, "IdentityApi"))
                .AddHttpMessageHandler<TimingHandler>();
            services.AddHttpClient<IApplicationUserService, ApplicationUserService>(c => ConfigureClient(c, identityBase, "IdentityApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IContactUsService, ContactUsService>(c => ConfigureClient(c, identityBase, "IdentityApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IRecipeService, RecipeService>(c => ConfigureClient(c, recipeBase, "RecipeBookApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>(c => ConfigureClient(c, recipeBase, "RecipeBookApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IProductService, ProductService>(c => ConfigureClient(c, recipeBase, "RecipeBookApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IProductCategoryService, ProductCategoryService>(c => ConfigureClient(c, recipeBase, "RecipeBookApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IUnitService, UnitService>(c => ConfigureClient(c, recipeBase, "RecipeBookApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IMealPlanService, MealPlanService>(c => ConfigureClient(c, mealPlannerBase, "MealPlannerApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IShopService, ShopService>(c => ConfigureClient(c, mealPlannerBase, "MealPlannerApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IShoppingListService, ShoppingListService>(c => ConfigureClient(c, mealPlannerBase, "MealPlannerApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();
            services.AddHttpClient<IStatisticsService, StatisticsService>(c => ConfigureClient(c, mealPlannerBase, "MealPlannerApi"))
                .AddHttpMessageHandler<TimingHandler>()
                .AddHttpMessageHandler<AuthRefreshHandler>();

            // ViewModels — Identity
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<ForgotPasswordViewModel>();
            services.AddTransient<ResetPasswordViewModel>();
            services.AddTransient<ChangePasswordViewModel>();
            services.AddTransient<UserProfileViewModel>();
            services.AddTransient<UsersOverviewViewModel>();
            services.AddTransient<ContactUsViewModel>();

            // ViewModels — RecipeBook
            services.AddTransient<RecipesOverviewViewModel>();
            services.AddTransient<RecipeDetailViewModel>();
            services.AddTransient<RecipeEditViewModel>();
            services.AddTransient<RecipeCategoriesViewModel>();
            services.AddTransient<RecipeCategoryEditViewModel>();
            services.AddTransient<ProductsOverviewViewModel>();
            services.AddTransient<ProductEditViewModel>();
            services.AddTransient<ProductCategoriesViewModel>();
            services.AddTransient<ProductCategoryEditViewModel>();
            services.AddTransient<UnitsOverviewViewModel>();
            services.AddTransient<UnitEditViewModel>();
            services.AddTransient<ProductStatisticsViewModel>();
            services.AddTransient<RecipeStatisticsViewModel>();

            // ViewModels — MealPlans
            services.AddTransient<MealPlansOverviewViewModel>();
            services.AddTransient<MealPlanEditViewModel>();
            services.AddTransient<ShopsOverviewViewModel>();
            services.AddTransient<ShopEditViewModel>();
            services.AddTransient<ShoppingListsOverviewViewModel>();
            services.AddTransient<ShoppingListEditViewModel>();

            // Pages — Startup
            services.AddTransient<StartupPage>();

            // Pages — Identity
            services.AddTransient<LoginPage>();
            services.AddTransient<RegisterPage>();
            services.AddTransient<ForgotPasswordPage>();
            services.AddTransient<ResetPasswordPage>();
            services.AddTransient<ChangePasswordPage>();
            services.AddTransient<UserProfilePage>();
            services.AddTransient<UsersOverviewPage>();
            services.AddTransient<ContactUsPage>();
            services.AddTransient<PrivacyPolicyPage>();

            // Pages — RecipeBook
            services.AddTransient<RecipesOverviewPage>();
            services.AddTransient<RecipeDetailPage>();
            services.AddTransient<RecipeEditPage>();
            services.AddTransient<RecipeCategoriesOverviewPage>();
            services.AddTransient<RecipeCategoryEditPage>();
            services.AddTransient<ProductsOverviewPage>();
            services.AddTransient<ProductEditPage>();
            services.AddTransient<ProductCategoriesOverviewPage>();
            services.AddTransient<ProductCategoryEditPage>();
            services.AddTransient<UnitsOverviewPage>();
            services.AddTransient<UnitEditPage>();
            services.AddTransient<ProductStatisticsPage>();
            services.AddTransient<RecipeStatisticsPage>();

            // Pages — MealPlans
            services.AddTransient<MealPlansOverviewPage>();
            services.AddTransient<MealPlanEditPage>();
            services.AddTransient<ShopsOverviewPage>();
            services.AddTransient<ShopEditPage>();
            services.AddTransient<ShoppingListsOverviewPage>();
            services.AddTransient<ShoppingListEditPage>();

            // Shell
            services.AddSingleton<AppShellViewModel>();
            services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
