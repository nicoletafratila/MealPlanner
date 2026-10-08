using CommunityToolkit.Maui.Extensions;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using MealPlanner.UI.Mobile.Views.Controls;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShopEditPage : ContentPage
    {
        private readonly ShopEditViewModel _vm;

        public ShopEditPage(ShopEditViewModel viewModel)
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
                    MealPlans.Resources.ShopEditPage.SelectShareUserTitle,
                    MealPlans.Resources.ShopEditPage.ShareNoUsersMessage,
                    MealPlans.Resources.ShopEditPage.OkButton);
                return;
            }

            var items = users.Select(u => new SelectorItem(u, DisplayName(u), u.Username)).ToList();
            var popup = new SelectorPopup(
                items,
                MealPlans.Resources.ShopEditPage.SelectShareUserTitle,
                MealPlans.Resources.ShopEditPage.PlaceholderSearchUsers,
                MealPlans.Resources.ShopEditPage.EmptyUsersLabel);
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
