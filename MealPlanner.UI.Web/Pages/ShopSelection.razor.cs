using Blazored.Modal;
using Blazored.Modal.Services;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopSelection : IComponent
    {
        public string? ShopId { get; set; }
        public IList<ShopModel>? Shops { get; set; }

        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; } = default!;

        [Inject]
        public IShopService? ShopService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Shops = await ShopService!.GetAllAsync();
            BlazoredModal.SetTitle("Select a shop");
        }

        private async Task SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(ShopId));
        }

        private async Task Cancel()
        {
            await BlazoredModal.CancelAsync();
        }
    }
}
