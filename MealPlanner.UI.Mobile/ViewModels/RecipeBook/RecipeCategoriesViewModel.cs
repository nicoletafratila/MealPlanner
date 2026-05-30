using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class RecipeCategoriesViewModel(RecipeCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<RecipeCategoryModel> _categories = [];

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await categoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 200 });
                if (result is not null)
                    Categories = new ObservableCollection<RecipeCategoryModel>(result.Items);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync("RecipeCategoryEdit?id=0");

        [RelayCommand]
        private Task EditAsync(RecipeCategoryModel cat) => Shell.Current.GoToAsync($"RecipeCategoryEdit?id={cat.Id}");

        [RelayCommand]
        private async Task DeleteAsync(RecipeCategoryModel cat)
        {
            var result = await categoryService.DeleteAsync(cat.Id);
            if (result?.Succeeded == true) Categories.Remove(cat);
            else SetError(result?.Message);
        }
    }
}
