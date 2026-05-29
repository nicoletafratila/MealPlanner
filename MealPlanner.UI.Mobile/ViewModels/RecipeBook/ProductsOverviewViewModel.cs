using System.Collections.ObjectModel;using Common.Pagination; using CommunityToolkit.Mvvm.ComponentModel; using CommunityToolkit.Mvvm.Input; using RecipeBook.Services.Core.Http; using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class ProductsOverviewViewModel(ProductService productService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<ProductModel> _products = [];
        [ObservableProperty] private string? _searchText;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _hasNextPage;

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true; CurrentPage = 1; ClearMessages();
            try
            {
                var filters = string.IsNullOrWhiteSpace(SearchText) ? null : new[] { new FilterItem("Name", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase) };
                var result = await productService.SearchAsync(new QueryParameters<ProductModel> { PageNumber = CurrentPage, PageSize = 20, Filters = filters });
                if (result is not null) { Products = new ObservableCollection<ProductModel>(result.Items); HasNextPage = result.Metadata.HasNextPage; }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand] private Task AddAsync() => Shell.Current.GoToAsync("ProductEdit?id=0");
        [RelayCommand] private Task EditAsync(ProductModel p) => Shell.Current.GoToAsync($"ProductEdit?id={p.Id}");

        [RelayCommand]
        private async Task DeleteAsync(ProductModel p)
        {
            var (success, error) = await productService.DeleteAsync(p.Id);
            if (success) Products.Remove(p);
            else SetError(error);
        }
    }
}
