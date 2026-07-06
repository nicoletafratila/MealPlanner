using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(CategoryId), "id")]
    public partial class ProductCategoryEditViewModel(ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty] private Guid _categoryId;
        [ObservableProperty] private ProductCategoryEditModel _model = new();
        [ObservableProperty] private bool _isNew;

        partial void OnCategoryIdChanged(Guid value)
        {
            IsNew = value == Guid.Empty;
            if (!IsNew)
            {
                _ = LoadAsync();
            }
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Model = await categoryService.GetEditAsync(CategoryId) ?? new();
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
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var result = IsNew ? await categoryService.AddAsync(Model) : await categoryService.UpdateAsync(Model);
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
