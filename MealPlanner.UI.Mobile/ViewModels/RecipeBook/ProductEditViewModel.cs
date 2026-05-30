using System.Collections.ObjectModel;using Common.Pagination; using CommunityToolkit.Mvvm.ComponentModel; using CommunityToolkit.Mvvm.Input; using RecipeBook.Services.Http; using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(ProductId), "id")]
    public partial class ProductEditViewModel(ProductService productService, ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private ProductEditModel _model = new();
        [ObservableProperty] private ObservableCollection<ProductCategoryModel> _categories = [];
        [ObservableProperty] private bool _isNew;

        partial void OnProductIdChanged(int value) { IsNew = value == 0; _ = LoadAsync(); }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var cats = await categoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 100 });
                if (cats is not null) Categories = new ObservableCollection<ProductCategoryModel>(cats.Items);
                if (!IsNew) Model = await productService.GetEditAsync(ProductId) ?? new();
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var result = IsNew ? await productService.AddAsync(Model) : await productService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }
    }
}
