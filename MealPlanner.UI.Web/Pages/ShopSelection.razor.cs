using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor.Charts;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopSelection : IComponent
    {
        [Required]
        public string? ShopId { get; set; }

        public PagedList<ShopModel>? Shops { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [CascadingParameter]
        protected BlazoredModalInstance BlazoredModal { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var queryParameters = new QueryParameters()
            {
                Filters = new List<FilterItem>(),
                SortString = "Name",
                SortDirection = SortDirection.Ascending,
                PageNumber = 1,
                PageSize = int.MaxValue,
            };
            Shops = await ShopService!.SearchAsync(queryParameters);
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
