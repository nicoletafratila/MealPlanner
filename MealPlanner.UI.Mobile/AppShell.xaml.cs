using MealPlanner.UI.Mobile.Pages.Identity;
using MealPlanner.UI.Mobile.Pages.MealPlans;
using MealPlanner.UI.Mobile.Pages.RecipeBook;

namespace MealPlanner.UI.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            Routing.RegisterRoute("RecipeDetail", typeof(RecipeDetailPage));
            Routing.RegisterRoute("RecipeEdit", typeof(RecipeEditPage));
            Routing.RegisterRoute("RecipeCategoryEdit", typeof(RecipeCategoryEditPage));
            Routing.RegisterRoute("ProductEdit", typeof(ProductEditPage));
            Routing.RegisterRoute("ProductCategoryEdit", typeof(ProductCategoryEditPage));
            Routing.RegisterRoute("UnitEdit", typeof(UnitEditPage));
            Routing.RegisterRoute("MealPlanEdit", typeof(MealPlanEditPage));
            Routing.RegisterRoute("ShopEdit", typeof(ShopEditPage));
            Routing.RegisterRoute("ShoppingListEdit", typeof(ShoppingListEditPage));
            Routing.RegisterRoute("ChangePassword", typeof(ChangePasswordPage));
            Routing.RegisterRoute("ForgotPassword", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("ResetPassword", typeof(ResetPasswordPage));
            Routing.RegisterRoute("Register", typeof(RegisterPage));
        }
    }
}
