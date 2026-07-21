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
    public partial class ProductEditViewModel(ProductService productService, ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty]
        private string _productId = string.Empty;

        [ObservableProperty]
        private ProductEditModel _model = new();

        [ObservableProperty]
        private ObservableCollection<ProductCategoryModel> _categories = [];

        [ObservableProperty]
        private ProductCategoryModel? _selectedCategory;

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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var cats = await categoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 100, Sorting = DefaultSorting });
                if (cats is not null) Categories = new ObservableCollection<ProductCategoryModel>(cats.Items);
                Guid.TryParse(ProductId, out var id);
                if (!IsNew) Model = await productService.GetEditAsync(id) ?? new();
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == Model.ProductCategoryId);
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
