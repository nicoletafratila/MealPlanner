using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class UnitsOverviewViewModel(UnitService unitService) : BaseViewModel
    {
        private const int PageSize = 200;

        [ObservableProperty]
        private ObservableCollection<UnitModel> _units = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _isLoadingMore;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;
            CurrentPage = 1;
            ClearMessages();
            try
            {
                var result = await unitService.SearchAsync(new QueryParameters<UnitModel> { PageNumber = CurrentPage, PageSize = PageSize, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Units = new ObservableCollection<UnitModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task NextPageAsync()
        {
            if (IsLoadingMore || IsBusy || !HasNextPage)
            {
                return;
            }
            IsLoadingMore = true;
            try
            {
                CurrentPage++;
                var result = await unitService.SearchAsync(new QueryParameters<UnitModel> { PageNumber = CurrentPage, PageSize = PageSize, Sorting = DefaultSorting });
                if (result is not null)
                {
                    foreach (var item in result.Items)
                    {
                        Units.Add(item);
                    }
                    HasNextPage = result.Metadata.HasNextPage;
                }
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync("UnitEdit?id=0");

        [RelayCommand]
        private Task EditAsync(UnitModel u) => Shell.Current.GoToAsync($"UnitEdit?id={u.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync(UnitModel u)
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await unitService.DeleteAsync(u.Id);
                if (result?.Succeeded == true)
                {
                    Units.Remove(u);
                }
                else
                {
                    SetError(result?.Message);
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
    }
}
