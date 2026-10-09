using CommunityToolkit.Maui.Extensions;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using MealPlanner.UI.Mobile.Views.Controls;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class ProductEditPage : ContentPage
    {
        private readonly ProductEditViewModel _vm;

        public ProductEditPage(ProductEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        private async void OnShareTapped(object sender, EventArgs e)
        {
            var users = await _vm.GetShareUsersAsync();
            if (users.Count == 0)
            {
                await this.DisplayAlertAsync(
                    RecipeBook.Resources.ProductEditPage.SelectShareUserTitle,
                    RecipeBook.Resources.ProductEditPage.ShareNoUsersMessage,
                    RecipeBook.Resources.ProductEditPage.OkButton);
                return;
            }

            var items = users.Select(u => new SelectorItem(u, DisplayName(u), u.Username)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.ProductEditPage.SelectShareUserTitle,
                RecipeBook.Resources.ProductEditPage.PlaceholderSearchUsers,
                RecipeBook.Resources.ProductEditPage.EmptyUsersLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ApplicationUserListModel user)
                await _vm.ShareToUserAsync(user.UserId);
        }

        private static string DisplayName(ApplicationUserListModel user)
        {
            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.IsNullOrWhiteSpace(fullName) ? user.Username : $"{fullName} ({user.Username})";
        }
    }
}
