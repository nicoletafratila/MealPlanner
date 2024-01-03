using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopSelection
    {
        public IList<ShopModel>? Shops { get; set; }

        private string? _shopId;
        public string? ShopId
        {
            get
            {
                return _shopId;
            }
            set
            {
                if (_shopId != value)
                {
                    _shopId = value;
                    OnShopChangedAsync(_shopId!);
                }
            }
        }

        [Parameter] public EventCallback<MouseEventArgs> OnClickCallback { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Shops = await ShopService!.GetAllAsync();
        }

        private void OnShopChangedAsync(string value)
        {
            ShopId = value;
            StateHasChanged();
        }
    }
}
