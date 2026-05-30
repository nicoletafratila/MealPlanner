using Common.Http;
using CommunityToolkit.Maui;
using Identity.Services.Http;
using MealPlanner.Services.Http;
using MealPlanner.UI.Mobile.Pages.Identity;
using MealPlanner.UI.Mobile.Pages.MealPlans;
using MealPlanner.UI.Mobile.Pages.RecipeBook;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Microsoft.Extensions.Configuration;
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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

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

            var services = builder.Services;

            // Infrastructure
            services.AddMemoryCache();
            services.AddSingleton<ITokenProvider, SecureStorageTokenProvider>();
            services.AddSingleton<MobileAuthStateService>();
            services.AddSingleton<IAuthStateNotifier>(sp => sp.GetRequiredService<MobileAuthStateService>());

            // API HTTP clients
            services.AddHttpClient<AuthenticationService>(c => c.BaseAddress = new Uri(identityBase));
            services.AddHttpClient<ApplicationUserService>(c => c.BaseAddress = new Uri(identityBase));
            services.AddHttpClient<RecipeService>(c => c.BaseAddress = new Uri(recipeBase));
            services.AddHttpClient<RecipeCategoryService>(c => c.BaseAddress = new Uri(recipeBase));
            services.AddHttpClient<ProductService>(c => c.BaseAddress = new Uri(recipeBase));
            services.AddHttpClient<ProductCategoryService>(c => c.BaseAddress = new Uri(recipeBase));
            services.AddHttpClient<UnitService>(c => c.BaseAddress = new Uri(recipeBase));
            services.AddHttpClient<IMealPlanService, MealPlanService>(c => c.BaseAddress = new Uri(mealPlannerBase));
            services.AddHttpClient<IShopService, ShopService>(c => c.BaseAddress = new Uri(mealPlannerBase));
            services.AddHttpClient<IShoppingListService, ShoppingListService>(c => c.BaseAddress = new Uri(mealPlannerBase));
            services.AddHttpClient<IStatisticsService, StatisticsService>(c => c.BaseAddress = new Uri(mealPlannerBase));

            // ViewModels — Identity
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<ForgotPasswordViewModel>();
            services.AddTransient<ResetPasswordViewModel>();
            services.AddTransient<ChangePasswordViewModel>();
            services.AddTransient<UserProfileViewModel>();
            services.AddTransient<UsersOverviewViewModel>();

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

            // Pages — Identity
            services.AddTransient<LoginPage>();
            services.AddTransient<RegisterPage>();
            services.AddTransient<ForgotPasswordPage>();
            services.AddTransient<ResetPasswordPage>();
            services.AddTransient<ChangePasswordPage>();
            services.AddTransient<UserProfilePage>();
            services.AddTransient<UsersOverviewPage>();

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
            services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
