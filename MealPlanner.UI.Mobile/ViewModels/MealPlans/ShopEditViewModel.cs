using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Core.Http;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(ShopId), "id")]
    public partial class ShopEditViewModel(ShopService shopService) : BaseViewModel
    {
        [ObservableProperty] private int _shopId;
        [ObservableProperty] private ShopEditModel _model = new();
        [ObservableProperty] private bool _isNew;

        partial void OnShopIdChanged(int value) { IsNew = value == 0; if (!IsNew) _ = LoadAsync(); }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try { Model = await shopService.GetEditAsync(ShopId) ?? new(); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var (success, error) = IsNew ? await shopService.AddAsync(Model) : await shopService.UpdateAsync(Model);
                if (success) await Shell.Current.GoToAsync("..");
                else SetError(error);
            }
            finally { IsBusy = false; }
        }
    }
}
