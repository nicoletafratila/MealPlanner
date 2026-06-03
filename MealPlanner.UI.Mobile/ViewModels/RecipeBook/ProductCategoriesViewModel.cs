using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class ProductCategoriesViewModel(ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<ProductCategoryModel> _categories = [];

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var result = await categoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 200, Sorting = DefaultSorting });
                if (result is not null) Categories = new ObservableCollection<ProductCategoryModel>(result.Items);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand] private Task AddAsync() => Shell.Current.GoToAsync($"ProductCategoryEdit?id={Guid.Empty}");
        [RelayCommand] private Task EditAsync(ProductCategoryModel c) => Shell.Current.GoToAsync($"ProductCategoryEdit?id={c.Id}");

        [RelayCommand]
        private async Task DeleteAsync(ProductCategoryModel c)
        {
            var result = await categoryService.DeleteAsync(c.Id);
            if (result?.Succeeded == true) Categories.Remove(c);
            else SetError(result?.Message);
        }
    }
}
