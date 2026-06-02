using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    public partial class ShoppingListsOverviewViewModel(IShoppingListService shoppingListService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<ShoppingListModel> _shoppingLists = [];
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _hasNextPage;

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return; IsBusy = true; CurrentPage = 1; ClearMessages();
            try
            {
                var result = await shoppingListService.SearchAsync(new QueryParameters<ShoppingListModel> { PageNumber = CurrentPage, Sorting = DefaultSorting });
                if (result is not null) { ShoppingLists = new ObservableCollection<ShoppingListModel>(result.Items); HasNextPage = result.Metadata.HasNextPage; }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand] private Task AddAsync() => Shell.Current.GoToAsync("ShoppingListEdit?id=0");
        [RelayCommand] private Task EditAsync(ShoppingListModel sl) => Shell.Current.GoToAsync($"ShoppingListEdit?id={sl.Id}");

        [RelayCommand]
        private async Task DeleteAsync(ShoppingListModel sl)
        {
            var result = await shoppingListService.DeleteAsync(sl.Id);
            if (result?.Succeeded == true) ShoppingLists.Remove(sl);
            else SetError(result?.Message);
        }
    }
}
