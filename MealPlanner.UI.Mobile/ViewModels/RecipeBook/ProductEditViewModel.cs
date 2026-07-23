using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(ProductId), "id")]
    public partial class ProductEditViewModel(ProductService productService, ProductCategoryService categoryService, UnitService unitService) : BaseViewModel
    {
        [ObservableProperty]
        private string _productId = string.Empty;

        [ObservableProperty]
        private ProductEditModel _model = new();

        [ObservableProperty]
        private ObservableCollection<ProductCategoryModel> _categories = [];

        [ObservableProperty]
        private ObservableCollection<UnitModel> _units = [];

        [ObservableProperty]
        private ProductCategoryModel? _selectedCategory;

        [ObservableProperty]
        private UnitModel? _selectedUnit;

        [ObservableProperty]
        private ImageSource? _productImage;

        [ObservableProperty]
        private bool _isNew;

        partial void OnProductIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            _ = LoadAsync();
        }

        partial void OnSelectedCategoryChanged(ProductCategoryModel? value)
        {
            if (value is not null) Model.ProductCategoryId = value.Id;
        }

        partial void OnSelectedUnitChanged(UnitModel? value)
        {
            if (value is not null) Model.BaseUnitId = value.Id;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var catTask = categoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 100, Sorting = DefaultSorting });
                var unitTask = unitService.SearchAsync(new QueryParameters<UnitModel> { PageSize = 100, Sorting = DefaultSorting });
                await Task.WhenAll(catTask, unitTask);

                if (catTask.Result is not null) Categories = new ObservableCollection<ProductCategoryModel>(catTask.Result.Items);
                if (unitTask.Result is not null) Units = new ObservableCollection<UnitModel>(unitTask.Result.Items);

                Guid.TryParse(ProductId, out var id);
                if (!IsNew) Model = await productService.GetEditAsync(id) ?? new();
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == Model.ProductCategoryId);
                SelectedUnit = Units.FirstOrDefault(u => u.Id == Model.BaseUnitId);
                if (Model.ImageContent is { Length: > 0 })
                    ProductImage = ImageSource.FromStream(() => new MemoryStream(Model.ImageContent));
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
        private async Task PickImageAsync()
        {
            try
            {
                var results = await MediaPicker.Default.PickPhotosAsync();
                var result = results?.FirstOrDefault();
                if (result is null) return;
                await using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Model.ImageContent = ms.ToArray();
                ProductImage = ImageSource.FromStream(() => new MemoryStream(Model.ImageContent));
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlertAsync(
                Pages.RecipeBook.Resources.ProductEditPage.DeleteConfirmTitle,
                Pages.RecipeBook.Resources.ProductEditPage.DeleteConfirmMessage,
                Pages.RecipeBook.Resources.ProductEditPage.DeleteButton,
                Pages.RecipeBook.Resources.ProductEditPage.CancelButton);
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                Guid.TryParse(ProductId, out var deleteId);
                var result = await productService.DeleteAsync(deleteId);
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
                SetError(RecipeBookSharedMessages.ProductNameRequired);
                return;
            }

            if (Model.ImageContent is not { Length: > 0 })
            {
                SetError(RecipeBookSharedMessages.ProductImageRequired);
                return;
            }

            if (SelectedCategory is null)
            {
                SetError(RecipeBookSharedMessages.ProductCategoryRequired);
                return;
            }

            IsBusy = true;
            try
            {
                var result = IsNew ? await productService.AddAsync(Model) : await productService.UpdateAsync(Model);
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
