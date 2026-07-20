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
        private const int PageSize = 200;

        [ObservableProperty]
        private ObservableCollection<RecipeCategoryModel> _categories = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _isLoadingMore;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            CurrentPage = 1;
            ClearMessages();
            try
            {
                var result = await categoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageNumber = CurrentPage, PageSize = PageSize, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Categories = new ObservableCollection<RecipeCategoryModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
                }
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
        private async Task NextPageAsync()
        {
            if (IsLoadingMore || IsBusy || !HasNextPage) return;
            IsLoadingMore = true;
            try
            {
                CurrentPage++;
                var result = await categoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageNumber = CurrentPage, PageSize = PageSize, Sorting = DefaultSorting });
                if (result is not null)
                {
                    foreach (var item in result.Items)
                    {
                        Categories.Add(item);
                    }
                    HasNextPage = result.Metadata.HasNextPage;
                }
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync($"RecipeCategoryEdit?id={Guid.Empty}");

        [RelayCommand]
        private Task EditAsync(RecipeCategoryModel cat) => Shell.Current.GoToAsync($"RecipeCategoryEdit?id={cat.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync(RecipeCategoryModel cat)
        {
            var result = await categoryService.DeleteAsync(cat.Id);
            if (result?.Succeeded == true) Categories.Remove(cat);
            else SetError(result?.Message);
        }
    }
}
