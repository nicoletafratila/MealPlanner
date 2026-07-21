using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(ShopId), "id")]
    public partial class ShopEditViewModel(IShopService shopService) : BaseViewModel
    {
        [ObservableProperty]
        private string _shopId = string.Empty;

        [ObservableProperty]
        private ShopEditModel _model = new();

        [ObservableProperty]
        private bool _isNew;

        partial void OnShopIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            if (!IsNew)
                _ = LoadAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Guid.TryParse(ShopId, out var id);
                Model = await shopService.GetEditAsync(id) ?? new();
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
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Name))
            {
                SetError(MealPlannerSharedMessages.ShopNameRequired);
                return;
            }

            if (Model.DisplaySequence is null || Model.DisplaySequence.Count == 0)
            {
                SetError(MealPlannerSharedMessages.ShopRequiresCategoryOrder);
                return;
            }

            IsBusy = true;
            try
            {
                var result = IsNew ? await shopService.AddAsync(Model) : await shopService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
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
