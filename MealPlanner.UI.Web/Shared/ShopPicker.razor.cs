using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MealPlanner.UI.Web.Shared
{
    public partial class ShopPicker
    {
        private string _filterText = string.Empty;

        [Parameter]
        public string Id { get; set; } = default!;

        [Parameter]
        public IEnumerable<ShopModel>? Shops { get; set; }

        [Parameter]
        public string? SelectedShopId { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = default!;

        [Parameter]
        public EventCallback<ChangeEventArgs> OnChanged { get; set; }

        private string FilterText => _filterText;

        private ShopModel? SelectedShop =>
            Shops?.FirstOrDefault(s => s.Id.ToString() == SelectedShopId);

        private IEnumerable<ShopModel>? FilteredShops =>
            Shops
                ?.Where(s => string.IsNullOrWhiteSpace(_filterText) || s.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase);

        private void OnFilterInput(ChangeEventArgs e)
        {
            _filterText = e.Value?.ToString() ?? string.Empty;
        }

        private async Task SelectAsync(ShopModel? item)
        {
            _filterText = string.Empty;
            var args = new ChangeEventArgs { Value = item?.Id.ToString() ?? string.Empty };
            await OnChanged.InvokeAsync(args);
        }
    }
}
