using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
using Blazored.Modal.Services;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopSelection : IComponent
    {
        [Required]
        public string? ShopId { get; set; }

        public IList<ShopModel>? Shops { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [CascadingParameter]
        protected BlazoredModalInstance BlazoredModal { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Shops = await ShopService!.GetAllAsync();
            BlazoredModal.SetTitle("Select a shop");
        }

        private async void SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(ShopId));
        }

        private async void CancelAsync()
        {
            await BlazoredModal.CancelAsync();
        }
    }
}
