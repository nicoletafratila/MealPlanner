using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Views.Controls
{
    public partial class ProductSelectorPopup : Popup<ProductModel>
    {
        private readonly IReadOnlyList<ProductModel> _allProducts;
        private readonly ObservableCollection<ProductModel> _filteredProducts;

        public ProductSelectorPopup(IReadOnlyList<ProductModel> products)
        {
            InitializeComponent();
            _allProducts = products;
            _filteredProducts = new ObservableCollection<ProductModel>(products);
            ProductsView.ItemsSource = _filteredProducts;
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var query = e.NewTextValue?.Trim();
            var matches = string.IsNullOrWhiteSpace(query)
                ? _allProducts
                : _allProducts.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            _filteredProducts.Clear();
            foreach (var product in matches)
                _filteredProducts.Add(product);
        }

        private async void OnProductSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ProductModel product)
                await CloseAsync(product, CancellationToken.None);
        }
    }
}
