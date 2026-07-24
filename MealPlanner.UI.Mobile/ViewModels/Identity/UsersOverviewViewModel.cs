using System.Collections.ObjectModel;
using Common.Models;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class UsersOverviewViewModel(ApplicationUserService userService) : BaseViewModel
    {
        private static readonly List<SortingModel> _defaultSorting = [new SortingModel { PropertyName = "Username", Direction = SortDirection.Ascending }];

        [ObservableProperty]
        private ObservableCollection<ApplicationUserModel> _users = [];

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _hasPreviousPage;

        partial void OnSearchTextChanged(string? value)
        {
            if (string.IsNullOrEmpty(value)) SearchCommand.Execute(null);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var filters = BuildFilters();
                var result = await userService.SearchAsync(new QueryParameters<ApplicationUserModel> { PageNumber = CurrentPage, Filters = filters, Sorting = _defaultSorting });
                if (result is not null)
                {
                    Users = new ObservableCollection<ApplicationUserModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
                    HasPreviousPage = result.Metadata.HasPreviousPage;
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private FilterItem[]? BuildFilters() =>
            string.IsNullOrWhiteSpace(SearchText) ? null : [new FilterItem("Username", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase)];

        [RelayCommand]
        private Task OpenUserAsync(ApplicationUserModel user) =>
            Shell.Current.GoToAsync($"UserDetail?username={Uri.EscapeDataString(user.Username)}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task UnlockUserAsync(string userId)
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            CommandResponse? result = null;
            try
            {
                result = await userService.UnlockAsync(userId);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }

            if (result?.Succeeded == true) await LoadAsync();
            else if (result is not null) SetError(result.Message);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task NextPageAsync()
        {
            CurrentPage++;
            await LoadAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadAsync();
            }
        }
    }
}
