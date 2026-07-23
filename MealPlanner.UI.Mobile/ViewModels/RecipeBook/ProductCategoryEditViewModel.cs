using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(CategoryId), "id")]
    public partial class ProductCategoryEditViewModel(ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty]
        private string _categoryId = string.Empty;

        [ObservableProperty]
        private ProductCategoryEditModel _model = new();

        [ObservableProperty]
        private bool _isNew;

        partial void OnCategoryIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            if (!IsNew)
            {
                _ = LoadAsync();
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Guid.TryParse(CategoryId, out var id);
                Model = await categoryService.GetEditAsync(id) ?? new();
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
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlertAsync(
                Pages.RecipeBook.Resources.ProductCategoryEditPage.DeleteConfirmTitle,
                Pages.RecipeBook.Resources.ProductCategoryEditPage.DeleteConfirmMessage,
                Pages.RecipeBook.Resources.ProductCategoryEditPage.DeleteButton,
                Pages.RecipeBook.Resources.ProductCategoryEditPage.CancelButton);
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                Guid.TryParse(CategoryId, out var deleteId);
                var result = await categoryService.DeleteAsync(deleteId);
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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Name))
            {
                SetError(RecipeBookSharedMessages.ProductCategoryNameRequired);
                return;
            }

            IsBusy = true;
            try
            {
                var result = IsNew ? await categoryService.AddAsync(Model) : await categoryService.UpdateAsync(Model);
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
