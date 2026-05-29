using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Core.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(UnitId), "id")]
    public partial class UnitEditViewModel(UnitService unitService) : BaseViewModel
    {
        [ObservableProperty] private int _unitId;
        [ObservableProperty] private UnitEditModel _model = new();
        [ObservableProperty] private bool _isNew;

        partial void OnUnitIdChanged(int value) { IsNew = value == 0; if (!IsNew) _ = LoadAsync(); }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try { Model = await unitService.GetEditAsync(UnitId) ?? new(); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var (success, error) = IsNew ? await unitService.AddAsync(Model) : await unitService.UpdateAsync(Model);
                if (success) await Shell.Current.GoToAsync("..");
                else SetError(error);
            }
            finally { IsBusy = false; }
        }
    }
}
