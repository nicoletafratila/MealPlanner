using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    public partial class ShopsOverviewViewModel(IShopService shopService) : BaseViewModel
    {
        private const int PageSize = 100;

        [ObservableProperty]
        private ObservableCollection<ShopModel> _shops = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _isLoadingMore;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            CurrentPage = 1;
            ClearMessages();
            try
            {
                var result = await shopService.SearchAsync(new QueryParameters<ShopModel> { PageNumber = CurrentPage, PageSize = PageSize, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Shops = new ObservableCollection<ShopModel>(result.Items);
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
            if (IsLoadingMore || IsBusy || !HasNextPage) return;
            IsLoadingMore = true;
            try
            {
                CurrentPage++;
                var result = await shopService.SearchAsync(new QueryParameters<ShopModel> { PageNumber = CurrentPage, PageSize = PageSize, Sorting = DefaultSorting });
                if (result is not null)
                {
                    foreach (var item in result.Items)
                    {
                        Shops.Add(item);
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
        private Task AddAsync() => Shell.Current.GoToAsync($"ShopEdit?id={Guid.Empty}");

        [RelayCommand]
        private Task EditAsync(ShopModel s) => Shell.Current.GoToAsync($"ShopEdit?id={s.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync(ShopModel s)
        {
            var result = await shopService.DeleteAsync(s.Id);
            if (result?.Succeeded == true)
            {
                Shops.Remove(s);
            }
            else
            {
                SetError(result?.Message);
            }
        }
    }
}
