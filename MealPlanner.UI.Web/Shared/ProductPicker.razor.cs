using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Shared
{
    public partial class ProductPicker
    {
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

        private ProductModel? SelectedProduct =>
            Products?.FirstOrDefault(p => p.Id.ToString() == SelectedProductId);

        private async Task SelectAsync(ProductModel? item)
        {
            var args = new ChangeEventArgs { Value = item?.Id.ToString() ?? "0" };
            await OnChanged.InvokeAsync(args);
        }
    }
}
