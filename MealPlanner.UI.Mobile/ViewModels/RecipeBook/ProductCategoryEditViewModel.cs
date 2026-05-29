using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Core.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(CategoryId), "id")]
    public partial class ProductCategoryEditViewModel(ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty] private int _categoryId;
        [ObservableProperty] private ProductCategoryEditModel _model = new();
        [ObservableProperty] private bool _isNew;

        partial void OnCategoryIdChanged(int value) { IsNew = value == 0; if (!IsNew) _ = LoadAsync(); }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try { Model = await categoryService.GetEditAsync(CategoryId) ?? new(); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var (success, error) = IsNew ? await categoryService.AddAsync(Model) : await categoryService.UpdateAsync(Model);
                if (success) await Shell.Current.GoToAsync("..");
                else SetError(error);
            }
            finally { IsBusy = false; }
        }
    }
}
