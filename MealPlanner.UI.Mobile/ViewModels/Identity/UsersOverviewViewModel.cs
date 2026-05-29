using System.Collections.ObjectModel;using Common.Pagination; using CommunityToolkit.Mvvm.ComponentModel; using CommunityToolkit.Mvvm.Input; using Identity.Services.Core.Http; using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class UsersOverviewViewModel(UserService userService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<ApplicationUserModel> _users = [];
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _hasNextPage;
        [ObservableProperty] private bool _hasPreviousPage;

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await userService.SearchAsync(new QueryParameters<ApplicationUserModel> { PageNumber = CurrentPage });
                if (result is not null)
                {
                    Users = new ObservableCollection<ApplicationUserModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
                    HasPreviousPage = result.Metadata.HasPreviousPage;
                }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task UnlockUserAsync(string userId)
        {
            var (success, error) = await userService.UnlockAsync(userId);
            if (success) await LoadAsync();
            else SetError(error);
        }

        [RelayCommand] private async Task NextPageAsync() { CurrentPage++; await LoadAsync(); }
        [RelayCommand] private async Task PreviousPageAsync() { if (CurrentPage > 1) { CurrentPage--; await LoadAsync(); } }
    }
}
