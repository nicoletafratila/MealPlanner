using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Shared
{
    public partial class ProductPicker
    {
        private string _filterText = string.Empty;

        [Parameter]
        public string Id { get; set; } = default!;

        [Parameter]
        public IEnumerable<ProductModel>? Products { get; set; }

        [Parameter]
        public string? SelectedProductId { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = default!;

        [Parameter]
        public EventCallback<ChangeEventArgs> OnChanged { get; set; }

        private string FilterText => _filterText;

        private ProductModel? SelectedProduct =>
            Products?.FirstOrDefault(p => p.Id.ToString() == SelectedProductId);

        private IEnumerable<ProductModel>? FilteredProducts =>
            Products is null || string.IsNullOrWhiteSpace(_filterText)
                ? Products
                : Products.Where(p => p.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase));

        private void OnFilterInput(ChangeEventArgs e)
        {
            _filterText = e.Value?.ToString() ?? string.Empty;
        }

        private async Task SelectAsync(ProductModel? item)
        {
            _filterText = string.Empty;
            var args = new ChangeEventArgs { Value = item?.Id.ToString() ?? "0" };
            await OnChanged.InvokeAsync(args);
        }
    }
}
