using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class ProductsOverviewViewModel(ProductService productService) : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<ProductModel> _products = [];

        [ObservableProperty]
        private string? _searchText;

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
            IsBusy = true; CurrentPage = 1; ClearMessages();
            try
            {
                var filters = BuildFilters();
                var result = await productService.SearchAsync(new QueryParameters<ProductModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Products = new ObservableCollection<ProductModel>(result.Items); HasNextPage = result.Metadata.HasNextPage;
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
                await AppendPageAsync();
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private async Task AppendPageAsync()
        {
            var filters = BuildFilters();
            var result = await productService.SearchAsync(new QueryParameters<ProductModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters, Sorting = DefaultSorting });
            if (result is not null)
            {
                foreach (var item in result.Items)
                {
                    Products.Add(item);
                }
                HasNextPage = result.Metadata.HasNextPage;
            }
        }

        private FilterItem[]? BuildFilters() =>
            string.IsNullOrWhiteSpace(SearchText) ? null : [new FilterItem("Name", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase)];

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync("ProductEdit?id=0");

        [RelayCommand]
        private Task EditAsync(ProductModel p) => Shell.Current.GoToAsync($"ProductEdit?id={p.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync(ProductModel p)
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await productService.DeleteAsync(p.Id);
                if (result?.Succeeded == true) Products.Remove(p);
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
