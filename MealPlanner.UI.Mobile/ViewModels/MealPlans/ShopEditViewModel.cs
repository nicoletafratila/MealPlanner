using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(ShopId), "id")]
    public partial class ShopEditViewModel(IShopService shopService) : BaseViewModel
    {
        [ObservableProperty] private Guid _shopId;
        [ObservableProperty] private ShopEditModel _model = new();
        [ObservableProperty] private bool _isNew;

        partial void OnShopIdChanged(Guid value)
        {
            IsNew = value == Guid.Empty;
            if (!IsNew)
                _ = LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Model = await shopService.GetEditAsync(ShopId) ?? new();
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
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = IsNew ? await shopService.AddAsync(Model) : await shopService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
