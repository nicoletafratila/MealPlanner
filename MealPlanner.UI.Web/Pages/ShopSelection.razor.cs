using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class ShopSelection : IComponent
    {
        [CascadingParameter]
        private BlazoredModalInstance BlazoredModal { get; set; } = default!;

        [Required]
        public string? ShopId { get; set; }

        public PagedList<ShopModel>? Shops { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Shops = await ShopService!.SearchAsync();
        }

        private async Task SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(ShopId));
        }

        private async Task CancelAsync()
        {
            await BlazoredModal.CancelAsync();
        }
    }
}
