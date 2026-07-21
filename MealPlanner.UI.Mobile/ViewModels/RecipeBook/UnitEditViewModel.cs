using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(UnitId), "id")]
    public partial class UnitEditViewModel(UnitService unitService) : BaseViewModel
    {
        [ObservableProperty]
        private string _unitId = string.Empty;

        [ObservableProperty]
        private UnitEditModel _model = new();

        [ObservableProperty]
        private bool _isNew;

        partial void OnUnitIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            if (!IsNew) _ = LoadAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Guid.TryParse(UnitId, out var id);
                Model = await unitService.GetEditAsync(id) ?? new();
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
                SetError(RecipeBookSharedMessages.UnitNameRequired);
                return;
            }

            if ((int)Model.UnitType is < 0 or > 3)
            {
                SetError(RecipeBookSharedMessages.UnitTypeRange);
                return;
            }

            IsBusy = true;
            try
            {
                var result = IsNew ? await unitService.AddAsync(Model) : await unitService.UpdateAsync(Model);
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
