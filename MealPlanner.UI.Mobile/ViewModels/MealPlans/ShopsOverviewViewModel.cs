using System.Collections.ObjectModel;using Common.Pagination; using CommunityToolkit.Mvvm.ComponentModel; using CommunityToolkit.Mvvm.Input; using MealPlanner.Services.Core.Http; using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    public partial class ShopsOverviewViewModel(ShopService shopService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<ShopModel> _shops = [];

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var result = await shopService.SearchAsync(new QueryParameters<ShopModel> { PageSize = 100 });
                if (result is not null) Shops = new ObservableCollection<ShopModel>(result.Items);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand] private Task AddAsync() => Shell.Current.GoToAsync("ShopEdit?id=0");
        [RelayCommand] private Task EditAsync(ShopModel s) => Shell.Current.GoToAsync($"ShopEdit?id={s.Id}");

        [RelayCommand]
        private async Task DeleteAsync(ShopModel s)
        {
            var (success, error) = await shopService.DeleteAsync(s.Id);
            if (success) Shops.Remove(s);
            else SetError(error);
        }
    }
}
