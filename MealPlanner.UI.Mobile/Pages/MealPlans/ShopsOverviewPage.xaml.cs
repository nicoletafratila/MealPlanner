using CommunityToolkit.Maui.Extensions;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using MealPlanner.UI.Mobile.Views.Controls;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShopsOverviewPage : ContentPage
    {
        private readonly ShopsOverviewViewModel _vm;

        public ShopsOverviewPage(ShopsOverviewViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCommand.Execute(null);
        }

        private async void OnShareAllTapped(object sender, EventArgs e)
        {
            var users = await _vm.GetShareUsersAsync();
            if (users.Count == 0)
            {
                await this.DisplayAlertAsync(
                    MealPlans.Resources.ShopsOverviewPage.ShareAllButton,
                    MealPlans.Resources.ShopsOverviewPage.ShareAllNoUsersMessage,
                    MealPlans.Resources.ShopsOverviewPage.OkButton);
                return;
            }

            var items = users.Select(u => new SelectorItem(u, DisplayName(u), u.Username)).ToList();
            var popup = new SelectorPopup(
                items,
                MealPlans.Resources.ShopsOverviewPage.ShareAllButton,
                MealPlans.Resources.ShopsOverviewPage.PlaceholderSearchUsers,
                MealPlans.Resources.ShopsOverviewPage.EmptyUsersLabel);
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
