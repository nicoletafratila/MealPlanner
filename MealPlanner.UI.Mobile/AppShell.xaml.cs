using System.ComponentModel;
using MealPlanner.UI.Mobile.Pages.Identity;
using MealPlanner.UI.Mobile.Pages.MealPlans;
using MealPlanner.UI.Mobile.Pages.RecipeBook;
using MealPlanner.UI.Mobile.ViewModels;

namespace MealPlanner.UI.Mobile
{
    public partial class AppShell : Shell
    {
        private readonly AppShellViewModel _viewModel;

        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            CurrentMenuHeader.BindingContext = viewModel;
            RegisterRoutes();
            PropertyChanged += OnShellPropertyChanged;
        }

        private async void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
            {
                await _viewModel.LoadCurrentCommand.ExecuteAsync(null);
            }
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
