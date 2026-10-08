using CommunityToolkit.Maui.Extensions;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using MealPlanner.UI.Mobile.Views.Controls;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipesOverviewPage : ContentPage
    {
        private readonly RecipesOverviewViewModel _vm;

        public RecipesOverviewPage(RecipesOverviewViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCommand.Execute(null);
        }

        private async void OnSelectCategoryTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.Categories.Where(c => c.Id != Guid.Empty).Select(c => new SelectorItem(c, c.Name)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipesOverviewPage.AllCategoriesTitle,
                RecipeBook.Resources.RecipesOverviewPage.PlaceholderSearchCategories,
                RecipeBook.Resources.RecipesOverviewPage.EmptyCategoriesLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is RecipeCategoryModel category)
                _vm.SelectedCategory = category;
        }

        private async void OnShareAllTapped(object sender, EventArgs e)
        {
            var users = await _vm.GetShareUsersAsync();
            if (users.Count == 0)
            {
                await this.DisplayAlertAsync(
                    RecipeBook.Resources.RecipesOverviewPage.ShareAllButton,
                    RecipeBook.Resources.RecipesOverviewPage.ShareAllNoUsersMessage,
                    RecipeBook.Resources.RecipesOverviewPage.OkButton);
                return;
            }

            var items = users.Select(u => new SelectorItem(u, DisplayName(u), u.Username)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipesOverviewPage.ShareAllButton,
                RecipeBook.Resources.RecipesOverviewPage.PlaceholderSearchUsers,
                RecipeBook.Resources.RecipesOverviewPage.EmptyUsersLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ApplicationUserListModel user)
                await _vm.ShareAllToUserAsync(user.UserId);
        }

        private static string DisplayName(ApplicationUserListModel user)
        {
            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.IsNullOrWhiteSpace(fullName) ? user.Username : $"{fullName} ({user.Username})";
        }
    }
}
