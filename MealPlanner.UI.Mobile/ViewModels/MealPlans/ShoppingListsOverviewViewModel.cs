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
        [ObservableProperty]
        private ObservableCollection<ShoppingListModel> _shoppingLists = [];

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await shoppingListService.SearchAsync(new QueryParameters<ShoppingListModel> { Sorting = DefaultSorting });
                if (result is not null)
                {
                    ShoppingLists = new ObservableCollection<ShoppingListModel>(result.Items);
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

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync($"ShoppingListEdit?id={Guid.Empty}");

        [RelayCommand]
        private Task EditAsync(ShoppingListModel sl) => Shell.Current.GoToAsync($"ShoppingListEdit?id={sl.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync(ShoppingListModel sl)
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await shoppingListService.DeleteAsync(sl.Id);
                if (result?.Succeeded == true)
                {
                    ShoppingLists.Remove(sl);
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
