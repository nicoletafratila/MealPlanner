using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShopSelection : ComponentBase
    {
        [CascadingParameter]
        public BlazoredModalInstance? ModalInstance { get; set; }

        public IModalController? ModalController { get; set; }

        [Required]
        public string? ShopId { get; set; }

        public PagedList<ShopModel> Shops { get; set; } = new([], new Metadata());

        [Inject]
        public IShopService ShopService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var result = await ShopService.SearchAsync();
            Shops = result ?? new PagedList<ShopModel>([], new Metadata());
        }

        protected override void OnParametersSet()
        {
            if (ModalController is null && ModalInstance is not null)
            {
                ModalController = new BlazoredModalController(ModalInstance);
            }
        }

        private async Task SaveAsync()
        {
            await ModalController!.CloseAsync(ShopId);
        }

        private async Task CancelAsync()
        {
            await ModalController!.CancelAsync();
        }
    }
}